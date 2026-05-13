using System;

namespace Features.Gameplay.CharacterController.Contracts
{
    public interface IPlayerHealth
    {
        event Action<int, int> HealthChanged;
        event Action<int> Damaged;
        event Action Died;

        int CurrentHealth { get; }
        int MaxHealth { get; }

        bool IsAlive { get; }
        bool IsInvulnerable { get; }

        bool TryTakeDamage(int amount);
        void Heal(int amount);
        void Kill();
        void ResetHealth();
    }
}