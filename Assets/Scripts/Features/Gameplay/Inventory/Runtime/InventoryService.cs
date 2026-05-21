using System;
using System.Collections.Generic;
using Core.Save;
using Cysharp.Threading.Tasks;
using Features.Gameplay.Inventory.Configs;
using Features.Gameplay.Inventory.Contracts;
using Features.Gameplay.Inventory.Data;

namespace Features.Gameplay.Inventory.Runtime
{
     public sealed class InventoryService : IInventoryService, ISaveDataProvider, IDisposable
    {
        private readonly IInventoryItemUseContext _useContext;
        private readonly IInventoryItemDatabase _itemDatabase;
        private readonly ISaveSystem _saveSystem;
        private readonly List<InventorySlotData> _slots;

        private bool _isDisposed;

        public event Action Changed;
        public event Action<InventoryItemDefinition, int> ItemAdded;

        public IReadOnlyList<InventorySlotData> Slots => _slots;
        public int Capacity => _slots.Count;

        public InventoryService(
            IInventoryItemUseContext useContext,
            IInventoryItemDatabase itemDatabase,
            ISaveSystem saveSystem)
        {
            _useContext = useContext ?? throw new ArgumentNullException(nameof(useContext));
            _itemDatabase = itemDatabase ?? throw new ArgumentNullException(nameof(itemDatabase));
            _saveSystem = saveSystem ?? throw new ArgumentNullException(nameof(saveSystem));

            _slots = new List<InventorySlotData>();

            EnsureCapacity(InventoryData.DefaultCapacity);

            _saveSystem.Register(this);
        }

        public UniTask LoadAsync(PersistentData data)
        {
            InventoryData inventoryData = data?.Gameplay?.Inventory ?? new InventoryData();

            int capacity = inventoryData.Capacity > 0
                ? inventoryData.Capacity
                : InventoryData.DefaultCapacity;

            EnsureCapacity(capacity);

            foreach (InventorySlotSaveData savedSlot in inventoryData.Slots)
            {
                if (savedSlot == null)
                    continue;

                if (!IsValidIndex(savedSlot.Index))
                    continue;

                if (string.IsNullOrWhiteSpace(savedSlot.ItemId))
                    continue;

                if (savedSlot.Amount <= 0)
                    continue;

                if (!_itemDatabase.TryGetItem(savedSlot.ItemId, out InventoryItemDefinition item))
                    continue;

                int amount = Math.Min(savedSlot.Amount, item.MaxStack);

                _slots[savedSlot.Index] = new InventorySlotData(item, amount);
            }

            Changed?.Invoke();

            return UniTask.CompletedTask;
        }

        public void Save(PersistentData data)
        {
            if (data == null)
                return;

            data.Gameplay.Inventory ??= new InventoryData();

            InventoryData inventoryData = data.Gameplay.Inventory;

            inventoryData.Capacity = _slots.Count;
            inventoryData.Slots.Clear();

            for (int i = 0; i < _slots.Count; i++)
            {
                InventorySlotData slot = _slots[i];

                if (slot.IsEmpty)
                    continue;

                inventoryData.Slots.Add(new InventorySlotSaveData
                {
                    Index = i,
                    ItemId = slot.Item.Id,
                    Amount = slot.Amount
                });
            }
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

            _slots[index] = slot.Amount <= 0
                ? InventorySlotData.Empty
                : slot;

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
        
        public int GetAmount(InventoryItemDefinition item)
        {
            if (item == null)
                return 0;

            int totalAmount = 0;

            for (int i = 0; i < _slots.Count; i++)
            {
                InventorySlotData slot = _slots[i];

                if (slot.IsEmpty)
                    continue;

                if (!string.Equals(slot.Item.Id, item.Id, StringComparison.Ordinal))
                    continue;

                totalAmount += slot.Amount;
            }

            return totalAmount;
        }

        public bool HasItem(InventoryItemDefinition item, int amount = 1)
        {
            if (item == null || amount <= 0)
                return false;

            return GetAmount(item) >= amount;
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

                if (!string.Equals(slot.Item.Id, item.Id, StringComparison.Ordinal))
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

        private void EnsureCapacity(int capacity)
        {
            capacity = Math.Max(1, capacity);

            _slots.Clear();

            for (int i = 0; i < capacity; i++)
                _slots.Add(InventorySlotData.Empty);
        }

        private bool IsValidIndex(int index)
        {
            return index >= 0 && index < _slots.Count;
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            _saveSystem.Unregister(this);
        }
    }
}