using System;
using Features.Gameplay.CharacterController.Contracts;
using UnityEngine;

namespace Features.Gameplay.CharacterController.Runtime
{
    [DisallowMultipleComponent]
    public sealed class PlayerBoostController : MonoBehaviour, IPlayerBoostController
    {
        private const float DefaultJumpMultiplier = 1f;

        private float _jumpBoostEndTime;

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
    }
}