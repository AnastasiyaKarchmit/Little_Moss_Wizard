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

        [Header("Jump")]
        [Tooltip("Initial upward velocity applied when jumping.")]
        [Min(0f)] public float JumpPower = 24f;

        [Tooltip("Maximum falling speed.")]
        [Min(0f)] public float MaxFallSpeed = 32f;

        [Tooltip("How quickly the player accelerates downward.")]
        [Min(0f)] public float FallAcceleration = 85f;

        [Tooltip("Extra gravity when the player releases jump early.")]
        [Min(1f)] public float JumpEndEarlyGravityModifier = 3f;

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

#if UNITY_EDITOR
        private void OnValidate()
        {
            JumpEndEarlyGravityModifier = Mathf.Max(1f, JumpEndEarlyGravityModifier);
            DashDuration = Mathf.Max(0.01f, DashDuration);
        }
#endif
    }
}