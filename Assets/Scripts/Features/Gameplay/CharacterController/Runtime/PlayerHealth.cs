using System;
using Features.Gameplay.CharacterController.Configs;
using Features.Gameplay.CharacterController.Contracts;
using UnityEngine;

namespace Features.Gameplay.CharacterController.Runtime
{
    [DisallowMultipleComponent]
    public sealed class PlayerHealth : MonoBehaviour, IPlayerHealth
    {
        [Header("Config")]
        [SerializeField] private PlayerHealthConfig config;

        private float _invulnerableUntilTime;
        private bool _isDead;

        public event Action<int, int> HealthChanged;
        public event Action<int> Damaged;
        public event Action Died;

        public int CurrentHealth { get; private set; }
        public int MaxHealth => config != null ? config.MaxHealth : 1;

        public bool IsAlive => !_isDead && CurrentHealth > 0;
        public bool IsInvulnerable => Time.time < _invulnerableUntilTime;

        private void Awake()
        {
            if (config == null)
            {
                Debug.LogError($"{nameof(PlayerHealthConfig)} is not assigned.", this);
                CurrentHealth = 1;
                return;
            }

            if (config.StartWithFullHealth)
                ResetHealth();
            else
                CurrentHealth = Mathf.Clamp(CurrentHealth, 1, config.MaxHealth);
        }

        public bool TryTakeDamage(int amount)
        {
            if (amount <= 0)
                return false;

            if (!IsAlive)
                return false;

            if (IsInvulnerable)
                return false;

            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);

            _invulnerableUntilTime = Time.time + config.InvulnerabilityTimeAfterHit;

            Damaged?.Invoke(amount);
            HealthChanged?.Invoke(CurrentHealth, MaxHealth);

            if (CurrentHealth <= 0)
                Die();

            return true;
        }

        public void Heal(int amount)
        {
            if (amount <= 0)
                return;

            if (!IsAlive)
                return;

            int previousHealth = CurrentHealth;

            CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);

            if (CurrentHealth != previousHealth)
                HealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }

        public void Kill()
        {
            if (!IsAlive)
                return;

            CurrentHealth = 0;
            HealthChanged?.Invoke(CurrentHealth, MaxHealth);
            Die();
        }

        public void ResetHealth()
        {
            _isDead = false;
            _invulnerableUntilTime = 0f;

            CurrentHealth = MaxHealth;
            HealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }

        private void Die()
        {
            if (_isDead)
                return;

            _isDead = true;
            Died?.Invoke();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (config == null)
            {
                Debug.LogWarning(
                    $"Assign {nameof(PlayerHealthConfig)} to {nameof(PlayerHealth)}.",
                    this);
            }
        }
#endif
    }
}