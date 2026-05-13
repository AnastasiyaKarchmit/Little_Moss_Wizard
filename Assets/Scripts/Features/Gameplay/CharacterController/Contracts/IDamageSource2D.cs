using UnityEngine;

namespace Features.Gameplay.CharacterController.Contracts
{
    public interface IDamageSource2D
    {
        int DamageAmount { get; }

        bool CanDamage { get; }

        Vector2 GetKnockbackDirection(Vector2 receiverPosition);

        float KnockbackForce { get; }
    }
}