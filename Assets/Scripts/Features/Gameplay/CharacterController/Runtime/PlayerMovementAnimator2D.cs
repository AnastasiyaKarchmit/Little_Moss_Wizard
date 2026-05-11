using UnityEngine;

namespace Features.Gameplay.CharacterController.Runtime
{
    [DisallowMultipleComponent]
    public sealed class PlayerMovementAnimator2D : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerMovementController2D movement;
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("Movement Blend")]
        [Tooltip("Horizontal speed that should be treated as full run animation speed.")]
        [SerializeField, Min(0.01f)] private float fullRunSpeed = 10f;

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

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            if (movement == null)
                ResolveReferences();

            if (movement == null)
                return;

            movement.Jumped += OnJumped;
            movement.Dashed += OnDashed;
            movement.GroundedChanged += OnGroundedChanged;
        }

        private void OnDisable()
        {
            if (movement == null)
                return;

            movement.Jumped -= OnJumped;
            movement.Dashed -= OnDashed;
            movement.GroundedChanged -= OnGroundedChanged;
        }

        private void Update()
        {
            if (movement == null || animator == null)
                return;

            UpdateMovementParameters();
            UpdateSpriteDirection();
        }

        private void UpdateMovementParameters()
        {
            Vector2 velocity = movement.Velocity;

            float horizontalSpeed = Mathf.Abs(velocity.x);

            if (horizontalSpeed < idleVelocityThreshold)
                horizontalSpeed = 0f;

            float normalizedVelocity = Mathf.Clamp01(horizontalSpeed / fullRunSpeed);

            animator.SetFloat(
                VelocityKey,
                normalizedVelocity,
                velocityDampTime,
                Time.deltaTime);

            animator.SetFloat(VerticalVelocityKey, velocity.y);
            animator.SetBool(GroundedKey, movement.Grounded);
            animator.SetBool(DashingKey, movement.IsDashing);
        }

        private void UpdateSpriteDirection()
        {
            if (!flipSprite || spriteRenderer == null)
                return;

            float inputX = movement.FrameInput.x;

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
            if (movement == null)
                movement = GetComponentInParent<PlayerMovementController2D>();

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