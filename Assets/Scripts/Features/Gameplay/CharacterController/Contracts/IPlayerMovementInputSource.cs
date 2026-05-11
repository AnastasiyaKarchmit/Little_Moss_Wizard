using Features.Gameplay.CharacterController.Data;

namespace Features.Gameplay.CharacterController.Contracts
{
    public interface IPlayerMovementInputSource
    {
        PlayerMovementInputFrame ConsumeFrameInput();
        void SetInputEnabled(bool enabled);
    }
}