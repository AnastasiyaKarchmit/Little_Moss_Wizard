using UnityEngine;

namespace Features.Gameplay.CharacterController.Runtime
{
    [DisallowMultipleComponent]
    public sealed class PlayerParticleController2D : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerMovementController2D movement;
        [SerializeField] private PlayerHealth health;
        [SerializeField] private PlayerBoostController boostController;

        [Header("Run Particles")]
        [SerializeField] private ParticleSystem runLeavesRightParticles;
        [SerializeField] private ParticleSystem runLeavesLeftParticles;

        [Header("Jump Particles")]
        [SerializeField] private ParticleSystem jumpStartParticles;

        [Header("Dash Particles")]
        [Tooltip("Played when the player dashes to the right.")]
        [SerializeField] private ParticleSystem dashRightParticles;

        [Tooltip("Played when the player dashes to the left. If empty, right particles will be used as fallback.")]
        [SerializeField] private ParticleSystem dashLeftParticles;

        [Header("Damage Particles")]
        [SerializeField] private ParticleSystem damageParticles;

        [Header("Potion Particles")]
        [SerializeField] private ParticleSystem healthPotionParticles;
        [SerializeField] private ParticleSystem jumpBoostPotionParticles;

        [Header("Run Leaves Settings")]
        [SerializeField, Min(0f)] private float minRunSpeed = 1.5f;
        [SerializeField, Min(0f)] private float stopDelay = 0.1f;
        [SerializeField] private bool playOnlyWhenGrounded = true;
        [SerializeField] private bool stopWhenDashing = true;

        private ParticleSystem _currentRunParticles;
        private bool _isRunEmitting;
        private float _lastRunWantedTime;

        private void Awake()
        {
            ResolveReferences();

            StopRunParticles(runLeavesRightParticles, true);
            StopRunParticles(runLeavesLeftParticles, true);

            StopOneShotParticles(jumpStartParticles, true);
            StopOneShotParticles(dashRightParticles, true);
            StopOneShotParticles(dashLeftParticles, true);
            StopOneShotParticles(damageParticles, true);
            StopOneShotParticles(healthPotionParticles, true);
            StopOneShotParticles(jumpBoostPotionParticles, true);
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();

            StopRunParticles(runLeavesRightParticles, true);
            StopRunParticles(runLeavesLeftParticles, true);

            _currentRunParticles = null;
            _isRunEmitting = false;
        }

        private void Update()
        {
            UpdateRunParticles();
        }

        private void Subscribe()
        {
            if (movement != null)
            {
                movement.Jumped += OnJumped;
                movement.Dashed += OnDashed;
            }

            if (health != null)
            {
                health.Healed += OnHealed;
                health.Damaged += OnDamaged;
            }

            if (boostController != null)
                boostController.JumpBoostApplied += OnJumpBoostApplied;
        }

        private void Unsubscribe()
        {
            if (movement != null)
            {
                movement.Jumped -= OnJumped;
                movement.Dashed -= OnDashed;
            }

            if (health != null)
            {
                health.Healed -= OnHealed;
                health.Damaged -= OnDamaged;
            }

            if (boostController != null)
                boostController.JumpBoostApplied -= OnJumpBoostApplied;
        }

        private void UpdateRunParticles()
        {
            if (movement == null)
                return;

            bool shouldEmit = ShouldEmitRunParticles();

            if (shouldEmit)
            {
                _lastRunWantedTime = Time.time;

                ParticleSystem wantedParticles = GetWantedRunParticles();

                if (wantedParticles != null)
                    SwitchRunParticles(wantedParticles);

                return;
            }

            if (Time.time >= _lastRunWantedTime + stopDelay)
                StopCurrentRunParticles(false);
        }

        private bool ShouldEmitRunParticles()
        {
            if (movement == null)
                return false;

            if (!movement.IsActive)
                return false;

            if (playOnlyWhenGrounded && !movement.Grounded)
                return false;

            if (stopWhenDashing && movement.IsDashing)
                return false;

            return Mathf.Abs(movement.Velocity.x) >= minRunSpeed;
        }

        private ParticleSystem GetWantedRunParticles()
        {
            float inputX = movement.FrameInput.x;

            if (Mathf.Abs(inputX) > 0.01f)
                return inputX > 0f ? runLeavesRightParticles : runLeavesLeftParticles;

            float velocityX = movement.Velocity.x;

            if (Mathf.Abs(velocityX) > 0.01f)
                return velocityX > 0f ? runLeavesRightParticles : runLeavesLeftParticles;

            return movement.FacingDirection >= 0
                ? runLeavesRightParticles
                : runLeavesLeftParticles;
        }

        private void SwitchRunParticles(ParticleSystem wantedParticles)
        {
            if (_currentRunParticles == wantedParticles && _isRunEmitting)
                return;

            if (_currentRunParticles != null && _currentRunParticles != wantedParticles)
                StopRunParticles(_currentRunParticles, false);

            _currentRunParticles = wantedParticles;
            _isRunEmitting = true;

            _currentRunParticles.Play(true);
        }

        private void StopCurrentRunParticles(bool clear)
        {
            if (_currentRunParticles == null)
                return;

            StopRunParticles(_currentRunParticles, clear);
            _isRunEmitting = false;
        }

        private static void StopRunParticles(ParticleSystem particles, bool clear)
        {
            if (particles == null)
                return;

            ParticleSystemStopBehavior stopBehavior = clear
                ? ParticleSystemStopBehavior.StopEmittingAndClear
                : ParticleSystemStopBehavior.StopEmitting;

            particles.Stop(true, stopBehavior);
        }

        private void OnJumped()
        {
            PlayOneShotParticles(jumpStartParticles);
        }

        private void OnDashed(Vector2 direction)
        {
            ParticleSystem dashParticles = GetDashParticles(direction);
            PlayOneShotParticles(dashParticles);
        }

        private void OnDamaged(int damageAmount)
        {
            PlayOneShotParticles(damageParticles);
        }

        private void OnHealed(int healAmount)
        {
            PlayOneShotParticles(healthPotionParticles);
        }

        private void OnJumpBoostApplied()
        {
            PlayOneShotParticles(jumpBoostPotionParticles);
        }

        private ParticleSystem GetDashParticles(Vector2 direction)
        {
            float directionX = direction.x;

            if (Mathf.Abs(directionX) <= 0.01f && movement != null)
                directionX = movement.FacingDirection;

            if (directionX < 0f)
                return dashLeftParticles != null ? dashLeftParticles : dashRightParticles;

            return dashRightParticles != null ? dashRightParticles : dashLeftParticles;
        }

        private static void PlayOneShotParticles(ParticleSystem particles)
        {
            if (particles == null)
                return;

            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particles.Play(true);
        }

        private static void StopOneShotParticles(ParticleSystem particles, bool clear)
        {
            if (particles == null)
                return;

            ParticleSystemStopBehavior stopBehavior = clear
                ? ParticleSystemStopBehavior.StopEmittingAndClear
                : ParticleSystemStopBehavior.StopEmitting;

            particles.Stop(true, stopBehavior);
        }

        private void ResolveReferences()
        {
            if (movement == null)
                movement = GetComponentInParent<PlayerMovementController2D>();

            if (health == null)
                health = GetComponentInParent<PlayerHealth>();

            if (boostController == null)
                boostController = GetComponentInParent<PlayerBoostController>();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ResolveReferences();
        }
#endif
    }
}