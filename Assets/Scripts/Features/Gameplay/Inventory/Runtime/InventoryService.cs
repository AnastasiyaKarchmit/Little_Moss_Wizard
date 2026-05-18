using System;
using System.Collections.Generic;
using Features.Gameplay.Inventory.Configs;
using Features.Gameplay.Inventory.Contracts;
using Features.Gameplay.Inventory.Data;

namespace Features.Gameplay.Inventory.Runtime
{
    public sealed class InventoryService : IInventoryService
    {
        private readonly IInventoryItemUseContext _useContext;
        private readonly List<InventorySlotData> _slots;

        public event Action Changed;
        public event Action<InventoryItemDefinition, int> ItemAdded;

        public IReadOnlyList<InventorySlotData> Slots => _slots;
        public int Capacity => _slots.Count;

        public InventoryService(IInventoryItemUseContext useContext)
        {
            _useContext = useContext;

            const int defaultCapacity = 12;

            _slots = new List<InventorySlotData>(defaultCapacity);

            for (int i = 0; i < defaultCapacity; i++)
                _slots.Add(InventorySlotData.Empty);
        }

        public bool AddItem(InventoryItemDefinition item, int amount = 1)
        {
            if (item == null || amount <= 0)
                return false;

            int remaining = amount;

            if (item.Stackable)
                remaining = AddToExistingStacks(item, remaining);

            remaining = AddToEmptySlots(item, remaining);

            int addedAmount = amount - remaining;

            if (addedAmount <= 0)
                return false;

            ItemAdded?.Invoke(item, addedAmount);
            Changed?.Invoke();

            return remaining <= 0;
        }

        public bool RemoveAt(int index, int amount = 1)
        {
            if (!IsValidIndex(index) || amount <= 0)
                return false;

            InventorySlotData slot = _slots[index];

            if (slot.IsEmpty)
                return false;

            slot.Amount -= amount;

            if (slot.Amount <= 0)
                _slots[index] = InventorySlotData.Empty;
            else
                _slots[index] = slot;

            Changed?.Invoke();

            return true;
        }

        public bool UseAt(int index)
        {
            if (!IsValidIndex(index))
                return false;

            InventorySlotData slot = _slots[index];

            if (slot.IsEmpty)
                return false;

            if (!slot.Item.CanUseItem(_useContext))
                return false;

            bool used = slot.Item.Use(_useContext);

            if (!used)
                return false;

            if (slot.Item.ConsumeOnUse)
                RemoveAt(index, 1);
            else
                Changed?.Invoke();

            return true;
        }

        public InventorySlotData GetSlot(int index)
        {
            return IsValidIndex(index)
                ? _slots[index]
                : InventorySlotData.Empty;
        }

        private int AddToExistingStacks(InventoryItemDefinition item, int amount)
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                if (amount <= 0)
                    break;

                InventorySlotData slot = _slots[i];

                if (slot.IsEmpty)
                    continue;

                if (slot.Item != item)
                    continue;

                if (slot.Amount >= item.MaxStack)
                    continue;

                int space = item.MaxStack - slot.Amount;
                int addAmount = Math.Min(space, amount);

                slot.Amount += addAmount;
                amount -= addAmount;

                _slots[i] = slot;
            }

            return amount;
        }

        private int AddToEmptySlots(InventoryItemDefinition item, int amount)
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                if (amount <= 0)
                    break;

                InventorySlotData slot = _slots[i];

                if (!slot.IsEmpty)
                    continue;

                int addAmount = item.Stackable
                    ? Math.Min(item.MaxStack, amount)
                    : 1;

                _slots[i] = new InventorySlotData(item, addAmount);
                amount -= addAmount;
            }

            return amount;
        }

        private bool IsValidIndex(int index)
        {
            return index >= 0 && index < _slots.Count;
        }
    }
}