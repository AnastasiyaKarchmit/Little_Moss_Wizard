using System;
using UnityEngine;

namespace Features.Gameplay.CharacterController.Contracts
{
    public interface IPlayerMovementController
    {
        event Action<bool, float> GroundedChanged;
        event Action Jumped;
        event Action<Vector2> Dashed;
        event Action DashEnded;

        Vector2 FrameInput { get; }
        Vector2 Velocity { get; }

        bool IsActive { get; }
        bool Grounded { get; }
        bool IsDashing { get; }

        int FacingDirection { get; }

        void SetActive(bool active);
    }
}