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
        [Header("Config")]
        [SerializeField] private PlayerMovementConfig config;

        private Rigidbody2D _rigidbody;
        private Collider2D _collider;

        private IPlayerMovementInputSource _inputSource;

        private ContactFilter2D _solidFilter;
        private readonly RaycastHit2D[] _castHits = new RaycastHit2D[8];

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
            _time <= _lastJumpPressedTime + config.JumpBuffer;

        private bool CanUseCoyote =>
            !_grounded &&
            _time <= _lastLeftGroundedTime + config.CoyoteTime;

        private bool HasBufferedDash =>
            _dashBuffered &&
            _time <= _lastDashPressedTime + config.DashBuffer;

        [Inject]
        public void Construct(IPlayerMovementInputSource inputSource)
        {
            _inputSource = inputSource;
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
            if (!_isActive || config == null)
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
            if (!_isActive || config == null)
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

        private void ResetRuntimeInput()
        {
            _input = default;

            _jumpBuffered = false;
            _dashBuffered = false;

            _endedJumpEarly = false;
            _isDashing = false;

            _lastJumpPressedTime = float.NegativeInfinity;
            _lastDashPressedTime = float.NegativeInfinity;
        }

        private void StopMovement()
        {
            _velocity = Vector2.zero;
            ApplyMovement();
        }

        private void ConfigureRigidbody()
        {
            _rigidbody.gravityScale = 0f;
            _rigidbody.freezeRotation = true;
            _rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        private void BuildCollisionFilter()
        {
            if (config == null)
                return;

            LayerMask collisionMask = config.SolidLayers.value != 0
                ? config.SolidLayers
                : ~config.PlayerLayer;

            _solidFilter = new ContactFilter2D();
            _solidFilter.SetLayerMask(collisionMask);
            _solidFilter.useTriggers = false;
        }

        private PlayerMovementInputFrame ProcessInput(PlayerMovementInputFrame rawInput)
        {
            Vector2 move = Vector2.ClampMagnitude(rawInput.Move, 1f);

            if (config.SnapInput)
            {
                move.x = Mathf.Abs(move.x) < config.HorizontalDeadZoneThreshold
                    ? 0f
                    : Mathf.Sign(move.x);

                move.y = Mathf.Abs(move.y) < config.VerticalDeadZoneThreshold
                    ? 0f
                    : Mathf.Sign(move.y);
            }
            else
            {
                if (Mathf.Abs(move.x) < config.HorizontalDeadZoneThreshold)
                    move.x = 0f;

                if (Mathf.Abs(move.y) < config.VerticalDeadZoneThreshold)
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

            bool groundHit = CastBody(Vector2.down, config.GroundCheckDistance);
            bool ceilingHit = CastBody(Vector2.up, config.GroundCheckDistance);

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
                if (_jumpBuffered && _time > _lastJumpPressedTime + config.JumpBuffer)
                    _jumpBuffered = false;

                return;
            }

            if (_grounded || CanUseCoyote)
                ExecuteJump();
        }
        
        private void HandleJumpSustain()
        {
            if (!config.UseRampedJumpStart)
                return;

            if (_grounded)
                return;

            if (_endedJumpEarly)
                return;

            if (!_input.JumpHeld)
                return;

            if (_velocity.y <= 0f)
                return;

            if (_time > _jumpStartedTime + config.JumpSustainTime)
                return;

            _velocity.y += config.JumpSustainAcceleration * Time.fixedDeltaTime;

            if (_velocity.y > config.InitialJumpVelocity)
                _velocity.y = config.InitialJumpVelocity;
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

            if (_time < _jumpStartedTime + config.MinJumpCutTime)
                return;

            _endedJumpEarly = true;

            _velocity.y *= config.JumpCutVelocityMultiplier;
        }

        private void ExecuteJump()
        {
            _jumpBuffered = false;

            _lastJumpPressedTime = float.NegativeInfinity;
            _lastLeftGroundedTime = float.NegativeInfinity;

            _endedJumpEarly = false;
            _jumpStartedTime = _time;

            _velocity.y = config.UseRampedJumpStart
                ? config.JumpStartVelocity
                : config.InitialJumpVelocity;

            Jumped?.Invoke();
        }

        // --------------------------------------------------------------------
        // Horizontal movement
        // --------------------------------------------------------------------

        private void HandleHorizontalMovement()
        {
            float targetSpeed = _input.Move.x * config.MaxSpeed;

            if (Mathf.Approximately(_input.Move.x, 0f))
            {
                float deceleration = _grounded
                    ? config.GroundDeceleration
                    : config.AirDeceleration;

                _velocity.x = Mathf.MoveTowards(
                    _velocity.x,
                    0f,
                    deceleration * Time.fixedDeltaTime);

                return;
            }

            _velocity.x = Mathf.MoveTowards(
                _velocity.x,
                targetSpeed,
                config.Acceleration * Time.fixedDeltaTime);
        }

        // --------------------------------------------------------------------
        // Gravity
        // --------------------------------------------------------------------

        private void HandleGravity()
        {
            if (_grounded && _velocity.y <= 0f)
            {
                _velocity.y = config.GroundingForce;
                return;
            }

            float gravity = config.Gravity;

            if (_velocity.y < 0f)
            {
                gravity *= config.FallGravityMultiplier;
            }
            else if (_endedJumpEarly && _velocity.y > 0f)
            {
                gravity *= config.GravityOnReleaseMultiplier;
            }

            _velocity.y += gravity * Time.fixedDeltaTime;

            if (_velocity.y < -config.MaxFallSpeed)
                _velocity.y = -config.MaxFallSpeed;
        }

        // --------------------------------------------------------------------
        // Dash
        // --------------------------------------------------------------------

        private bool HandleDash()
        {
            if (!config.DashEnabled)
                return false;

            if (_isDashing)
            {
                if (_time >= _dashStartedTime + config.DashDuration)
                {
                    EndDash();
                    return false;
                }

                _velocity = _dashDirection * config.DashSpeed;
                return true;
            }

            if (!HasBufferedDash)
            {
                if (_dashBuffered && _time > _lastDashPressedTime + config.DashBuffer)
                    _dashBuffered = false;

                return false;
            }

            if (!CanStartDash())
                return false;

            StartDash();
            return true;
        }

        private bool CanStartDash()
        {
            bool cooldownReady =
                _time >= _lastDashStartedTime + config.DashCooldown;

            bool hasDashAvailable =
                _grounded || _airDashesUsed < config.AirDashes;

            return cooldownReady && hasDashAvailable;
        }

        private void StartDash()
        {
            _dashBuffered = false;

            _isDashing = true;

            _dashStartedTime = _time;
            _lastDashStartedTime = _time;

            _dashDirection = GetDashDirection();

            if (!_grounded)
                _airDashesUsed++;

            _endedJumpEarly = false;

            _velocity = _dashDirection * config.DashSpeed;

            Dashed?.Invoke(_dashDirection);
        }

        private void EndDash()
        {
            _isDashing = false;

            _velocity.x *= config.DashEndSpeedMultiplier;
            _velocity.y = Mathf.Min(_velocity.y, 0f);

            DashEnded?.Invoke();
        }

        private Vector2 GetDashDirection()
        {
            Vector2 direction = _input.Move;

            if (!config.AllowVerticalDash)
                direction.y = 0f;

            if (direction.sqrMagnitude < 0.01f)
                direction = Vector2.right * _facingDirection;

            if (!config.AllowVerticalDash)
                direction.x = direction.x >= 0f ? 1f : -1f;

            return direction.normalized;
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
            SetBodyVelocity(_velocity);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (config == null)
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