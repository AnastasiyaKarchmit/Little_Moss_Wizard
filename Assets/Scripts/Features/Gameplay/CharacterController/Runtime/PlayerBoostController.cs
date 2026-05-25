using System;
using Features.Gameplay.CharacterController.Contracts;
using UnityEngine;
using VContainer;

namespace Features.Gameplay.CharacterController.Runtime
{
    [DisallowMultipleComponent]
    public sealed class PlayerBoostController : MonoBehaviour, IPlayerBoostController
    {
        private const float DefaultJumpMultiplier = 1f;
        
        private IPlayerHealth _playerHealth;

        private float _jumpBoostEndTime;
        private bool _subscribed;

        public event Action JumpBoostChanged;
        public event Action JumpBoostApplied;
        public event Action JumpBoostCleared;

        public bool HasJumpBoost { get; private set; }

        public float JumpHeightMultiplier { get; private set; } = DefaultJumpMultiplier;

        public float JumpBoostRemainingTime
        {
            get
            {
                if (!HasJumpBoost)
                    return 0f;

                return Mathf.Max(0f, _jumpBoostEndTime - Time.time);
            }
        }
        
        [Inject]
        public void Construct(IPlayerHealth playerHealth)
        {
            _playerHealth = playerHealth;
            SubscribeToEvents();
        }
        
        private void OnEnable() => SubscribeToEvents();
        
        private void OnDisable() => UnsubscribeFromEvents();

        private void Update()
        {
            if (!HasJumpBoost)
                return;

            if (Time.time < _jumpBoostEndTime)
                return;

            ClearJumpBoost();
        }

        public void ApplyJumpBoost(float heightMultiplier, float duration)
        {
            heightMultiplier = Mathf.Max(DefaultJumpMultiplier, heightMultiplier);
            duration = Mathf.Max(0f, duration);

            JumpHeightMultiplier = heightMultiplier;
            HasJumpBoost = true;

            _jumpBoostEndTime = Time.time + duration;

            JumpBoostApplied?.Invoke();
            JumpBoostChanged?.Invoke();
        }

        public void ClearJumpBoost()
        {
            if (!HasJumpBoost && Mathf.Approximately(JumpHeightMultiplier, DefaultJumpMultiplier))
                return;

            HasJumpBoost = false;
            JumpHeightMultiplier = DefaultJumpMultiplier;
            _jumpBoostEndTime = 0f;

            JumpBoostCleared?.Invoke();
            JumpBoostChanged?.Invoke();
        }

        private void SubscribeToEvents()
        {
            if (_subscribed || _playerHealth == null)
                return;

            _subscribed = true;
            _playerHealth.Died += ClearJumpBoost;
        }

        private void UnsubscribeFromEvents()
        {
            if (_playerHealth == null)
                return;
            
            _playerHealth.Died  -= ClearJumpBoost;
           _subscribed = false;
        }
    }
}