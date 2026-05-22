using System;
using Features.Gameplay.CharacterController.Configs;
using Features.Gameplay.CharacterController.Contracts;
using Features.Gameplay.CharacterController.Data;
using UnityEngine;
using VContainer;

namespace Features.Gameplay.CharacterController.Runtime
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class PlayerMovementController2D : MonoBehaviour, IPlayerMovementController
    {
        [Header("Dash Collision Protection")]
        [SerializeField, Min(0.001f)] private float dashWallCheckSkin = 0.08f;
        [SerializeField, Range(-1f, 0f)] private float dashWallNormalDotThreshold = -0.2f;
        
        private PlayerMovementConfig _config;
        private IPlayerBoostController _boostController;

        private Rigidbody2D _rigidbody;
        private Collider2D _collider;

        private IPlayerMovementInputSource _inputSource;

        private ContactFilter2D _solidFilter;
        private readonly RaycastHit2D[] _castHits = new RaycastHit2D[16];

        private PlayerMovementInputFrame _input;
        private Vector2 _velocity;

        private float _time;
        private int _facingDirection = 1;

        private bool _isActive = true;
        private bool _grounded;
        private bool _endedJumpEarly;

        private bool _jumpBuffered;
        private float _jumpStartedTime = float.NegativeInfinity;
        private float _lastJumpPressedTime = float.NegativeInfinity;
        private float _lastLeftGroundedTime = float.NegativeInfinity;

        private bool _dashBuffered;
        private bool _isDashing;
        private Vector2 _dashDirection;
        private float _dashStartedTime = float.NegativeInfinity;
        private float _lastDashStartedTime = float.NegativeInfinity;
        private float _lastDashPressedTime = float.NegativeInfinity;
        private int _airDashesUsed;
        
        private Vector2 _externalVelocity;
        
        private float CurrentJumpHeightMultiplier =>
            _boostController != null
                ? Mathf.Max(1f, _boostController.JumpHeightMultiplier)
                : 1f;

        private float CurrentJumpVelocityMultiplier =>
            Mathf.Sqrt(CurrentJumpHeightMultiplier);

        private float CurrentInitialJumpVelocity =>
            _config.InitialJumpVelocity * CurrentJumpVelocityMultiplier;

        private float CurrentJumpStartVelocity =>
            _config.JumpStartVelocity * CurrentJumpVelocityMultiplier;

        private float CurrentJumpSustainAcceleration =>
            _config.JumpSustainAcceleration * CurrentJumpVelocityMultiplier;

        public event Action<bool, float> GroundedChanged;
        public event Action Jumped;
        public event Action<Vector2> Dashed;
        public event Action DashEnded;

        public Vector2 FrameInput => _input.Move;
        public Vector2 Velocity => _velocity;

        public bool IsActive => _isActive;
        public bool Grounded => _grounded;
        public bool IsDashing => _isDashing;

        public int FacingDirection => _facingDirection;

        private bool HasBufferedJump =>
            _jumpBuffered &&
            _time <= _lastJumpPressedTime + _config.JumpBuffer;

        private bool CanUseCoyote =>
            !_grounded &&
            _time <= _lastLeftGroundedTime + _config.CoyoteTime;

        private bool HasBufferedDash =>
            _dashBuffered &&
            _time <= _lastDashPressedTime + _config.DashBuffer;

        [Inject]
        public void Construct(
            IPlayerMovementInputSource inputSource,
            PlayerMovementConfig config,
            IPlayerBoostController boostController)
        {
            _inputSource = inputSource;
            _config = config;
            _boostController = boostController;
        }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();

            ConfigureRigidbody();
            BuildCollisionFilter();

            _velocity = GetBodyVelocity();
        }

        private void Update()
        {
            if (!_isActive || _config == null)
                return;

            _time += Time.deltaTime;

            if (_inputSource == null)
                return;

            _input = ProcessInput(_inputSource.ConsumeFrameInput());

            if (Mathf.Abs(_input.Move.x) > 0f)
                _facingDirection = _input.Move.x > 0f ? 1 : -1;

            if (_input.JumpPressed)
            {
                _jumpBuffered = true;
                _lastJumpPressedTime = _time;
            }

            if (_input.DashPressed)
            {
                _dashBuffered = true;
                _lastDashPressedTime = _time;
            }
        }

        private void FixedUpdate()
        {
            if (!_isActive || _config == null)
                return;

            _velocity = GetBodyVelocity();

            CheckMovementCollisions();

            if (HandleDash())
            {
                ApplyMovement();
                return;
            }

            HandleJump();
            HandleHorizontalMovement();
            HandleGravity();

            ApplyMovement();
        }

        public void SetActive(bool active)
        {
            if (_isActive == active)
                return;

            _isActive = active;

            _inputSource?.SetInputEnabled(active);

            if (!active)
            {
                ResetRuntimeInput();
                StopMovement();
            }
        }
        
        public void AddExternalVelocity(Vector2 velocity)
        {
            _externalVelocity += velocity;
        }

        public void SetExternalVelocity(Vector2 velocity)
        {
            _externalVelocity = velocity;
        }

        public void ClearExternalVelocity()
        {
            _externalVelocity = Vector2.zero;
        }

        private void ResetRuntimeInput()
        {
            _input = default;

            _jumpBuffered = false;
            _dashBuffered = false;

            _endedJumpEarly = false;
            _isDashing = false;

            _externalVelocity = Vector2.zero;

            _lastJumpPressedTime = float.NegativeInfinity;
            _lastDashPressedTime = float.NegativeInfinity;
        }

        private void StopMovement()
        {
            _velocity = Vector2.zero;
            _externalVelocity = Vector2.zero;
            SetBodyVelocity(Vector2.zero);
        }

        private void ConfigureRigidbody()
        {
            _rigidbody.gravityScale = 0f;
            _rigidbody.freezeRotation = true;
            _rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        private void BuildCollisionFilter()
        {
            if (_config == null)
                return;

            LayerMask collisionMask = _config.SolidLayers.value != 0
                ? _config.SolidLayers
                : ~_config.PlayerLayer;

            _solidFilter = new ContactFilter2D();
            _solidFilter.SetLayerMask(collisionMask);
            _solidFilter.useTriggers = false;
        }

        private PlayerMovementInputFrame ProcessInput(PlayerMovementInputFrame rawInput)
        {
            Vector2 move = Vector2.ClampMagnitude(rawInput.Move, 1f);

            if (_config.SnapInput)
            {
                move.x = Mathf.Abs(move.x) < _config.HorizontalDeadZoneThreshold
                    ? 0f
                    : Mathf.Sign(move.x);

                move.y = Mathf.Abs(move.y) < _config.VerticalDeadZoneThreshold
                    ? 0f
                    : Mathf.Sign(move.y);
            }
            else
            {
                if (Mathf.Abs(move.x) < _config.HorizontalDeadZoneThreshold)
                    move.x = 0f;

                if (Mathf.Abs(move.y) < _config.VerticalDeadZoneThreshold)
                    move.y = 0f;
            }

            return new PlayerMovementInputFrame(
                move,
                rawInput.JumpPressed,
                rawInput.JumpHeld,
                rawInput.DashPressed);
        }

        // --------------------------------------------------------------------
        // Movement collisions
        // --------------------------------------------------------------------

        private void CheckMovementCollisions()
        {
            bool wasGrounded = _grounded;

            bool groundHit = CastBody(Vector2.down, _config.GroundCheckDistance);
            bool ceilingHit = CastBody(Vector2.up, _config.GroundCheckDistance);

            if (ceilingHit && _velocity.y > 0f)
                _velocity.y = 0f;

            if (groundHit)
            {
                _grounded = true;

                if (!wasGrounded)
                {
                    _airDashesUsed = 0;
                    _endedJumpEarly = false;

                    GroundedChanged?.Invoke(true, Mathf.Abs(_velocity.y));
                }
            }
            else
            {
                _grounded = false;

                if (wasGrounded)
                {
                    _lastLeftGroundedTime = _time;
                    GroundedChanged?.Invoke(false, 0f);
                }
            }
        }

        private bool CastBody(Vector2 direction, float distance)
        {
            int hitCount = _collider.Cast(
                direction,
                _solidFilter,
                _castHits,
                distance,
                true);

            return hitCount > 0;
        }

        // --------------------------------------------------------------------
        // Jump
        // --------------------------------------------------------------------

        private void HandleJump()
        {
            HandleJumpCut();
            HandleJumpSustain();

            if (!HasBufferedJump)
            {
                if (_jumpBuffered && _time > _lastJumpPressedTime + _config.JumpBuffer)
                    _jumpBuffered = false;

                return;
            }

            if (_grounded || CanUseCoyote)
                ExecuteJump();
        }
        
        private void HandleJumpSustain()
        {
            if (!_config.UseRampedJumpStart)
                return;

            if (_grounded)
                return;

            if (_endedJumpEarly)
                return;

            if (!_input.JumpHeld)
                return;

            if (_velocity.y <= 0f)
                return;

            if (_time > _jumpStartedTime + _config.JumpSustainTime)
                return;

            _velocity.y += CurrentJumpSustainAcceleration * Time.fixedDeltaTime;

            if (_velocity.y > CurrentInitialJumpVelocity)
                _velocity.y = CurrentInitialJumpVelocity;
        }
        
        private void HandleJumpCut()
        {
            if (_grounded)
                return;

            if (_endedJumpEarly)
                return;

            if (_input.JumpHeld)
                return;

            if (_velocity.y <= 0f)
                return;

            if (_time < _jumpStartedTime + _config.MinJumpCutTime)
                return;

            _endedJumpEarly = true;

            _velocity.y *= _config.JumpCutVelocityMultiplier;
        }

        private void ExecuteJump()
        {
            _jumpBuffered = false;

            _lastJumpPressedTime = float.NegativeInfinity;
            _lastLeftGroundedTime = float.NegativeInfinity;

            _endedJumpEarly = false;
            _jumpStartedTime = _time;

            _velocity.y = _config.UseRampedJumpStart
                ? CurrentJumpStartVelocity
                : CurrentInitialJumpVelocity;

            Jumped?.Invoke();
        }

        // --------------------------------------------------------------------
        // Horizontal movement
        // --------------------------------------------------------------------

        private void HandleHorizontalMovement()
        {
            float targetSpeed = _input.Move.x * _config.MaxSpeed;

            if (Mathf.Approximately(_input.Move.x, 0f))
            {
                float deceleration = _grounded
                    ? _config.GroundDeceleration
                    : _config.AirDeceleration;

                _velocity.x = Mathf.MoveTowards(
                    _velocity.x,
                    0f,
                    deceleration * Time.fixedDeltaTime);

                return;
            }

            _velocity.x = Mathf.MoveTowards(
                _velocity.x,
                targetSpeed,
                _config.Acceleration * Time.fixedDeltaTime);
        }

        // --------------------------------------------------------------------
        // Gravity
        // --------------------------------------------------------------------

        private void HandleGravity()
        {
            if (_grounded && _velocity.y <= 0f)
            {
                _velocity.y = _config.GroundingForce;
                return;
            }

            float gravity = _config.Gravity;

            if (_velocity.y < 0f)
            {
                gravity *= _config.FallGravityMultiplier;
            }
            else if (_endedJumpEarly && _velocity.y > 0f)
            {
                gravity *= _config.GravityOnReleaseMultiplier;
            }

            _velocity.y += gravity * Time.fixedDeltaTime;

            if (_velocity.y < -_config.MaxFallSpeed)
                _velocity.y = -_config.MaxFallSpeed;
        }

        // --------------------------------------------------------------------
        // Dash
        // --------------------------------------------------------------------

        private bool HandleDash()
        {
            if (!_config.DashEnabled)
                return false;

            if (_isDashing)
            {
                if (_time >= _dashStartedTime + _config.DashDuration)
                {
                    EndDash();
                    return false;
                }

                if (IsDashBlockedThisFrame())
                {
                    EndDashAgainstWall();
                    return false;
                }

                _velocity = _dashDirection * _config.DashSpeed;
                return true;
            }

            if (!HasBufferedDash)
            {
                if (_dashBuffered && _time > _lastDashPressedTime + _config.DashBuffer)
                    _dashBuffered = false;

                return false;
            }

            if (!CanStartDash())
                return false;

            return TryStartDash();
        }

        private bool CanStartDash()
        {
            bool cooldownReady =
                _time >= _lastDashStartedTime + _config.DashCooldown;

            bool hasDashAvailable =
                _grounded || _airDashesUsed < _config.AirDashes;

            return cooldownReady && hasDashAvailable;
        }

        private bool TryStartDash()
        {
            _dashBuffered = false;

            Vector2 dashDirection = GetDashDirection();

            if (IsDashBlocked(dashDirection))
            {
                _velocity.x = 0f;
                return false;
            }

            _isDashing = true;

            _dashStartedTime = _time;
            _lastDashStartedTime = _time;

            _dashDirection = dashDirection;

            if (!_grounded)
                _airDashesUsed++;

            _endedJumpEarly = false;

            _velocity = _dashDirection * _config.DashSpeed;

            Dashed?.Invoke(_dashDirection);

            return true;
        }

        private void EndDash()
        {
            _isDashing = false;

            _velocity.x *= _config.DashEndSpeedMultiplier;
            _velocity.y = Mathf.Min(_velocity.y, 0f);

            DashEnded?.Invoke();
        }

        private Vector2 GetDashDirection()
        {
            Vector2 direction = _input.Move;

            if (!_config.AllowVerticalDash)
                direction.y = 0f;

            if (direction.sqrMagnitude < 0.01f)
                direction = Vector2.right * _facingDirection;

            if (!_config.AllowVerticalDash)
                direction.x = direction.x >= 0f ? 1f : -1f;

            return direction.normalized;
        }
        
        private bool IsDashBlockedThisFrame()
        {
            return IsDashBlocked(_dashDirection);
        }

        private bool IsDashBlocked(Vector2 dashDirection)
        {
            if (dashDirection.sqrMagnitude <= 0.0001f)
                return false;

            float checkDistance =
                _config.DashSpeed * Time.fixedDeltaTime + dashWallCheckSkin;

            int hitCount = _collider.Cast(
                dashDirection,
                _solidFilter,
                _castHits,
                checkDistance,
                true);

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit2D hit = _castHits[i];

                if (hit.collider == null)
                    continue;

                if (hit.collider == _collider)
                    continue;

                if (hit.collider.isTrigger)
                    continue;

                float directionIntoSurface = Vector2.Dot(dashDirection, hit.normal);

                if (directionIntoSurface <= dashWallNormalDotThreshold)
                    return true;
            }

            return false;
        }

        private void EndDashAgainstWall()
        {
            _isDashing = false;

            _velocity = Vector2.zero;
            _externalVelocity = Vector2.zero;

            SetBodyVelocity(Vector2.zero);

            DashEnded?.Invoke();
        }

        // --------------------------------------------------------------------
        // Rigidbody velocity compatibility
        // --------------------------------------------------------------------

        private Vector2 GetBodyVelocity()
        {
#if UNITY_6000_0_OR_NEWER
            return _rigidbody.linearVelocity;
#else
            return _rigidbody.velocity;
#endif
        }

        private void SetBodyVelocity(Vector2 velocity)
        {
#if UNITY_6000_0_OR_NEWER
            _rigidbody.linearVelocity = velocity;
#else
            _rigidbody.velocity = velocity;
#endif
        }

        private void ApplyMovement()
        {
            SetBodyVelocity(_velocity + _externalVelocity);

            _externalVelocity = Vector2.MoveTowards(
                _externalVelocity,
                Vector2.zero,
                80f * Time.fixedDeltaTime);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_config == null)
            {
                Debug.LogWarning(
                    $"Assign {nameof(PlayerMovementConfig)} to {nameof(PlayerMovementController2D)}.",
                    this);
            }

            if (_rigidbody == null)
                _rigidbody = GetComponent<Rigidbody2D>();

            if (_rigidbody != null)
            {
                _rigidbody.gravityScale = 0f;
                _rigidbody.freezeRotation = true;
            }
        }
#endif
    }
}