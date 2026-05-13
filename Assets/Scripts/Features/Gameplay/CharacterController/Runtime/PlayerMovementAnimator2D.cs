using Features.Gameplay.CharacterController.Configs;
using UnityEngine;
using VContainer;

namespace Features.Gameplay.CharacterController.Runtime
{
    [DisallowMultipleComponent]
    public sealed class PlayerMovementAnimator2D : MonoBehaviour
    {
        [Header("References")] 
        
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;
        
        [Tooltip("Smoothing time for the Velocity blend tree parameter.")]
        [SerializeField, Min(0f)] private float velocityDampTime = 0.08f;

        [Tooltip("Small velocity ignored by the animator to prevent tiny idle jitter.")]
        [SerializeField, Min(0f)] private float idleVelocityThreshold = 0.05f;

        [Header("Sprite")]
        [SerializeField] private bool flipSprite = true;

        private static readonly int VelocityKey = Animator.StringToHash("Velocity");
        private static readonly int VerticalVelocityKey = Animator.StringToHash("VerticalVelocity");
        private static readonly int GroundedKey = Animator.StringToHash("Grounded");
        private static readonly int DashingKey = Animator.StringToHash("Dashing");

        private static readonly int JumpKey = Animator.StringToHash("Jump");
        private static readonly int DashKey = Animator.StringToHash("Dash");
        private static readonly int LandKey = Animator.StringToHash("Land");
        
        private PlayerMovementController2D _movement;
        private float _fullRunSpeed = 10f;

        [Inject]
        public void Construct(PlayerMovementController2D movementController, PlayerMovementConfig config)
        {
            _movement = movementController;
            _fullRunSpeed = config.MaxSpeed;
        }
        
        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            if (_movement == null)
                ResolveReferences();

            if (_movement == null)
                return;

            _movement.Jumped += OnJumped;
            _movement.Dashed += OnDashed;
            _movement.GroundedChanged += OnGroundedChanged;
        }

        private void OnDisable()
        {
            if (_movement == null)
                return;

            _movement.Jumped -= OnJumped;
            _movement.Dashed -= OnDashed;
            _movement.GroundedChanged -= OnGroundedChanged;
        }

        private void Update()
        {
            if (_movement == null || animator == null)
                return;

            UpdateMovementParameters();
            UpdateSpriteDirection();
        }

        private void UpdateMovementParameters()
        {
            Vector2 velocity = _movement.Velocity;

            float horizontalSpeed = Mathf.Abs(velocity.x);

            if (horizontalSpeed < idleVelocityThreshold)
                horizontalSpeed = 0f;

            float normalizedVelocity = Mathf.Clamp01(horizontalSpeed / _fullRunSpeed);

            animator.SetFloat(
                VelocityKey,
                normalizedVelocity,
                velocityDampTime,
                Time.deltaTime);

            animator.SetFloat(VerticalVelocityKey, velocity.y);
            animator.SetBool(GroundedKey, _movement.Grounded);
            animator.SetBool(DashingKey, _movement.IsDashing);
        }

        private void UpdateSpriteDirection()
        {
            if (!flipSprite || spriteRenderer == null)
                return;

            float inputX = _movement.FrameInput.x;

            if (Mathf.Abs(inputX) <= 0.01f)
                return;

            spriteRenderer.flipX = inputX < 0f;
        }

        private void OnJumped()
        {
            if (animator == null)
                return;

            animator.ResetTrigger(LandKey);
            animator.SetTrigger(JumpKey);
        }

        private void OnDashed(Vector2 direction)
        {
            if (animator == null)
                return;

            animator.SetTrigger(DashKey);
        }

        private void OnGroundedChanged(bool grounded, float impactVelocity)
        {
            if (animator == null)
                return;

            animator.SetBool(GroundedKey, grounded);

            if (!grounded)
                return;

            animator.ResetTrigger(JumpKey);
            animator.SetTrigger(LandKey);
        }

        private void ResolveReferences()
        {
            if (_movement == null)
                _movement = GetComponentInParent<PlayerMovementController2D>();

            if (animator == null)
                animator = GetComponentInChildren<Animator>();

            if (spriteRenderer == null)
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ResolveReferences();
        }
#endif
    }
}