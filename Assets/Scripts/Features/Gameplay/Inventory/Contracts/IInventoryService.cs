using System;
using System.Collections.Generic;
using Features.Gameplay.Inventory.Configs;
using Features.Gameplay.Inventory.Data;

namespace Features.Gameplay.Inventory.Contracts
{
    public interface IInventoryService
    {
        event Action Changed;

        IReadOnlyList<InventorySlotData> Slots { get; }

        int Capacity { get; }

        bool AddItem(InventoryItemDefinition item, int amount = 1);
        bool RemoveAt(int index, int amount = 1);
        bool UseAt(int index);

        InventorySlotData GetSlot(int index);
    }
}