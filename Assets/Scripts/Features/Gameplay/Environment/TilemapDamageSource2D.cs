using Features.Gameplay.CharacterController.Contracts;
using UnityEngine;

namespace Features.Gameplay.Environment
{
    [DisallowMultipleComponent]
    public sealed class TilemapDamageSource2D : MonoBehaviour, IDamageSource2D
    {
        [Header("Damage")]
        [SerializeField, Min(1)] private int damageAmount = 1;

        [Header("Knockback")]
        [SerializeField, Min(0f)] private float knockbackForce = 8f;

        [Tooltip("Good default for spikes/lava: knock player upward.")]
        [SerializeField] private Vector2 knockbackDirection = Vector2.up;

        public int DamageAmount => damageAmount;
        public bool CanDamage => isActiveAndEnabled;
        public float KnockbackForce => knockbackForce;

        public Vector2 GetKnockbackDirection(Vector2 receiverPosition)
        {
            if (knockbackDirection.sqrMagnitude < 0.01f)
                return Vector2.up;

            return knockbackDirection.normalized;
        }
    }
}