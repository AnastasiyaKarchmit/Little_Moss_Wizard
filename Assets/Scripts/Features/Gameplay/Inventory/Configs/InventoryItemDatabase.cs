using System.Collections.Generic;
using Features.Gameplay.Inventory.Contracts;
using UnityEngine;

namespace Features.Gameplay.Inventory.Configs
{
    [CreateAssetMenu(
        fileName = "InventoryItemDatabase",
        menuName = "Configs/Inventory/Item Database")]
    public sealed class InventoryItemDatabase : ScriptableObject, IInventoryItemDatabase
    {
        [SerializeField] private List<InventoryItemDefinition> items = new();

        private Dictionary<string, InventoryItemDefinition> _itemsById;

        public bool TryGetItem(string id, out InventoryItemDefinition item)
        {
            EnsureInitialized();

            if (string.IsNullOrWhiteSpace(id))
            {
                item = null;
                return false;
            }

            return _itemsById.TryGetValue(id, out item);
        }

        private void EnsureInitialized()
        {
            if (_itemsById != null)
                return;

            _itemsById = new Dictionary<string, InventoryItemDefinition>();

            foreach (InventoryItemDefinition item in items)
            {
                if (item == null)
                    continue;

                if (!_itemsById.TryAdd(item.Id, item))
                {
                    Debug.LogWarning(
                        $"Duplicate inventory item id: {item.Id}",
                        item);
                }
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _itemsById = null;
        }
#endif
    }
}