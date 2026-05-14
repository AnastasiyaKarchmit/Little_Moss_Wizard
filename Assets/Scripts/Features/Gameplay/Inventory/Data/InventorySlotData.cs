using Features.Gameplay.Inventory.Configs;

namespace Features.Gameplay.Inventory.Data
{
    public struct InventorySlotData
    {
        public InventoryItemDefinition Item;
        public int Amount;

        public bool IsEmpty => Item == null || Amount <= 0;

        public InventorySlotData(InventoryItemDefinition item, int amount)
        {
            Item = item;
            Amount = amount;
        }

        public static InventorySlotData Empty => new(null, 0);
    }
}