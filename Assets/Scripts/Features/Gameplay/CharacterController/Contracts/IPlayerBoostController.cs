using System;

namespace Features.Gameplay.CharacterController.Contracts
{
    public interface IPlayerBoostController
    {
        event Action JumpBoostChanged;

        bool HasJumpBoost { get; }
        float JumpHeightMultiplier { get; }
        float JumpBoostRemainingTime { get; }

        void ApplyJumpBoost(float heightMultiplier, float duration);
        void ClearJumpBoost();
    }
}