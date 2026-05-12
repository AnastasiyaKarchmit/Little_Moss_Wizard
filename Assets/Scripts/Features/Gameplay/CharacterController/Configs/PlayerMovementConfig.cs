using UnityEngine;

namespace Features.Gameplay.CharacterController.Configs
{
    [CreateAssetMenu(
        fileName = "PlayerMovementConfig",
        menuName = "Configs/Player/Movement Config")]
    public sealed class PlayerMovementConfig : ScriptableObject
    {
        [Header("Layers")]
        [Tooltip("The player's own layer. Used as fallback if Solid Layers is empty.")]
        public LayerMask PlayerLayer;

        [Tooltip("Layers treated as solid ground, platforms, walls, and ceilings.")]
        public LayerMask SolidLayers;

        [Header("Input")]
        [Tooltip("Snaps movement input to -1, 0, or 1.")]
        public bool SnapInput = true;

        [Range(0.01f, 0.99f)]
        public float HorizontalDeadZoneThreshold = 0.1f;

        [Range(0.01f, 0.99f)]
        public float VerticalDeadZoneThreshold = 0.3f;

        [Header("Movement")]
        [Tooltip("Default horizontal movement speed. The player is always running.")]
        [Min(0f)] public float MaxSpeed = 10f;

        [Tooltip("How quickly the player reaches target horizontal speed.")]
        [Min(0f)] public float Acceleration = 100f;

        [Tooltip("How quickly the player stops while grounded.")]
        [Min(0f)] public float GroundDeceleration = 70f;

        [Tooltip("How quickly the player stops in air when no horizontal input is held.")]
        [Min(0f)] public float AirDeceleration = 35f;

        [Tooltip("Small downward force while grounded. Helps the player stay grounded on slopes.")]
        [Range(-10f, 0f)] public float GroundingForce = -1.5f;

        [Tooltip("Distance used for ground and ceiling checks.")]
        [Range(0.01f, 0.5f)] public float GroundCheckDistance = 0.05f;

        [Header("Jump Shape")]
        [Tooltip("Desired full jump height in Unity units.")]
        [Min(0.1f)] public float JumpHeight = 4.75f;

        [Tooltip("Time in seconds until the player reaches the top of the jump.")]
        [Range(0.1f, 1f)] public float TimeToJumpApex = 0.42f;

        [Tooltip("Small compensation because collision and fixed timestep can slightly reduce real jump height.")]
        [Range(1f, 1.2f)] public float JumpHeightCompensationFactor = 1.04f;

        [Tooltip("Maximum falling speed.")]
        [Min(0f)] public float MaxFallSpeed = 28f;
        
        [Header("Jump Start Feel")]
        [Tooltip("If enabled, jump starts with partial velocity and receives the rest while jump is held.")]
        public bool UseRampedJumpStart = true;

        [Tooltip("How much of the calculated jump velocity is applied immediately.")]
        [Range(0.1f, 1f)]
        public float JumpStartVelocityMultiplier = 0.65f;

        [Tooltip("How long the controller can add extra upward force after jump start.")]
        [Range(0.01f, 0.25f)]
        public float JumpSustainTime = 0.12f;

        [Header("Variable Jump Height")]
        [Tooltip("When jump is released early, upward velocity is multiplied by this value.")]
        [Range(0.1f, 1f)] public float JumpCutVelocityMultiplier = 0.45f;

        [Tooltip("Extra gravity while moving upward after jump was released.")]
        [Range(1f, 8f)] public float GravityOnReleaseMultiplier = 2.2f;

        [Tooltip("Extra gravity when falling. Makes the jump less floaty on the way down.")]
        [Range(1f, 8f)] public float FallGravityMultiplier = 1.35f;

        [Tooltip("Prevents accidental instant jump cut on the same physics tick as jump start.")]
        [Range(0f, 0.1f)] public float MinJumpCutTime = 0.035f;

        [Header("Jump Forgiveness")]
        [Tooltip("Allows jumping shortly after leaving a platform.")]
        [Min(0f)] public float CoyoteTime = 0.12f;

        [Tooltip("Allows jump input shortly before landing.")]
        [Min(0f)] public float JumpBuffer = 0.15f;

        [Header("Dash")]
        public bool DashEnabled = true;

        [Tooltip("Horizontal dash speed.")]
        [Min(0f)] public float DashSpeed = 22f;

        [Tooltip("How long the dash lasts.")]
        [Min(0.01f)] public float DashDuration = 0.13f;

        [Tooltip("Minimum time between dashes.")]
        [Min(0f)] public float DashCooldown = 0.18f;

        [Tooltip("Allows dash input shortly before dash becomes available.")]
        [Min(0f)] public float DashBuffer = 0.12f;

        [Tooltip("How many air dashes are allowed before landing.")]
        [Min(0)] public int AirDashes = 1;

        [Tooltip("For Hollow Knight-like movement, keep this false.")]
        public bool AllowVerticalDash = false;

        [Tooltip("How much horizontal speed remains after dash ends.")]
        [Range(0f, 1f)] public float DashEndSpeedMultiplier = 0.55f;

        public float Gravity { get; private set; }
        public float InitialJumpVelocity { get; private set; }
        public float JumpStartVelocity { get; private set; }
        public float JumpSustainAcceleration { get; private set; }
        public float AdjustedJumpHeight { get; private set; }

        private void OnEnable()
        {
            CalculateJumpValues();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            TimeToJumpApex = Mathf.Max(0.1f, TimeToJumpApex);
            JumpHeight = Mathf.Max(0.1f, JumpHeight);
            JumpHeightCompensationFactor = Mathf.Max(1f, JumpHeightCompensationFactor);
            JumpSustainTime = Mathf.Max(0.01f, JumpSustainTime);

            DashDuration = Mathf.Max(0.01f, DashDuration);

            CalculateJumpValues();
        }
#endif

        private void CalculateJumpValues()
        {
            AdjustedJumpHeight = JumpHeight * JumpHeightCompensationFactor;

            Gravity = -(2f * AdjustedJumpHeight) / Mathf.Pow(TimeToJumpApex, 2f);
            InitialJumpVelocity = Mathf.Abs(Gravity) * TimeToJumpApex;

            JumpStartVelocity = InitialJumpVelocity * JumpStartVelocityMultiplier;

            if (JumpSustainTime > 0f)
            {
                float missingVelocity = InitialJumpVelocity - JumpStartVelocity;

                // Add Mathf.Abs(Gravity) so the sustain force can fight gravity too.
                JumpSustainAcceleration =
                    missingVelocity / JumpSustainTime + Mathf.Abs(Gravity);
            }
            else
            {
                JumpSustainAcceleration = 0f;
            }
        }
    }
}