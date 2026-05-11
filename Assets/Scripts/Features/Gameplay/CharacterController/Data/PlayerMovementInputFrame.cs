using UnityEngine;

namespace Features.Gameplay.CharacterController.Data
{
    public readonly struct PlayerMovementInputFrame
    {
        public readonly Vector2 Move;

        public readonly bool JumpPressed;
        public readonly bool JumpHeld;

        public readonly bool DashPressed;

        public PlayerMovementInputFrame(
            Vector2 move,
            bool jumpPressed,
            bool jumpHeld,
            bool dashPressed)
        {
            Move = move;

            JumpPressed = jumpPressed;
            JumpHeld = jumpHeld;

            DashPressed = dashPressed;
        }
    }

}