using Features.Gameplay.CharacterController.Contracts;

namespace Features.Gameplay.Inventory.Contracts
{
    public interface IInventoryItemUseContext
    {
        IPlayerHealth PlayerHealth { get; }
        IPlayerBoostController PlayerBoosts { get; }
    }
}