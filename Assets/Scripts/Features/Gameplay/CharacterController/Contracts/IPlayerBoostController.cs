using System;

namespace Features.Gameplay.CharacterController.Contracts
{
    public interface IPlayerBoostController
    {
        event Action JumpBoostChanged;
        event Action JumpBoostApplied;
        event Action JumpBoostCleared;

        bool HasJumpBoost { get; }
        float JumpHeightMultiplier { get; }
        float JumpBoostRemainingTime { get; }

        void ApplyJumpBoost(float heightMultiplier, float duration);
        void ClearJumpBoost();
    }
}