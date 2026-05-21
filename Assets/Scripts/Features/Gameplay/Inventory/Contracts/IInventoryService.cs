using System;
using System.Collections.Generic;
using Features.Gameplay.Inventory.Configs;
using Features.Gameplay.Inventory.Data;

namespace Features.Gameplay.Inventory.Contracts
{
    public interface IInventoryService
    {
        event Action Changed;
        
        event Action<InventoryItemDefinition, int> ItemAdded;

        IReadOnlyList<InventorySlotData> Slots { get; }

        int Capacity { get; }

        bool AddItem(InventoryItemDefinition item, int amount = 1);
        bool RemoveAt(int index, int amount = 1);
        bool UseAt(int index);
        
        int GetAmount(InventoryItemDefinition item);
        bool HasItem(InventoryItemDefinition item, int amount = 1);

        InventorySlotData GetSlot(int index);
    }
}