using Features.Gameplay.CharacterController.Contracts;
using UnityEngine;

namespace Features.Gameplay.CharacterController.Runtime
{
    [DisallowMultipleComponent]
    public sealed class PlayerDamageReceiver2D : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerHealth health;
        [SerializeField] private PlayerMovementController2D movement;

        [Header("Knockback")]
        [SerializeField] private bool applyKnockback = true;
        [SerializeField, Min(0f)] private float verticalKnockbackBonus = 3f;

        public bool TryReceiveDamage(IDamageSource2D damageSource, Vector2 receiverPosition)
        {
            if (damageSource == null)
                return false;

            if (!damageSource.CanDamage)
                return false;

            if (health == null)
                ResolveReferences();

            if (health == null)
                return false;

            bool damaged = health.TryTakeDamage(damageSource.DamageAmount);

            if (!damaged)
                return false;

            if (applyKnockback)
                ApplyKnockback(damageSource, receiverPosition);

            return true;
        }

        private void ApplyKnockback(IDamageSource2D damageSource, Vector2 receiverPosition)
        {
            if (movement == null)
                ResolveReferences();

            if (movement == null)
                return;

            Vector2 direction = damageSource.GetKnockbackDirection(receiverPosition);

            if (direction.sqrMagnitude < 0.01f)
                direction = Vector2.up;

            direction.Normalize();

            Vector2 knockback = direction * damageSource.KnockbackForce;
            knockback.y += verticalKnockbackBonus;

            movement.SetExternalVelocity(knockback);
        }

        private void Awake()
        {
            ResolveReferences();
        }

        private void ResolveReferences()
        {
            if (health == null)
                health = GetComponentInParent<PlayerHealth>();

            if (movement == null)
                movement = GetComponentInParent<PlayerMovementController2D>();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ResolveReferences();
        }
#endif
    }
}