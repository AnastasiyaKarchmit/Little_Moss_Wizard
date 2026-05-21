using Features.Gameplay.Inventory.Configs;

namespace Features.Gameplay.Inventory.Contracts
{
    public interface IInventoryItemDatabase
    {
        bool TryGetItem(string id, out InventoryItemDefinition item);
    }
}