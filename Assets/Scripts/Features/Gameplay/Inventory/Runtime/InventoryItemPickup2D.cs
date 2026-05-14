using Features.Gameplay.CharacterController.Runtime;
using Features.Gameplay.Inventory.Configs;
using Features.Gameplay.Inventory.Contracts;
using UnityEngine;
using VContainer;

namespace Features.Gameplay.Inventory.Runtime
{
    [DisallowMultipleComponent]
    public sealed class InventoryItemPickup2D : MonoBehaviour
    {
        [SerializeField] private InventoryItemDefinition item;
        [SerializeField, Min(1)] private int amount = 1;
        [SerializeField] private bool destroyAfterPickup = true;

        private IInventoryService _inventoryService;

        [Inject]
        public void Construct(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_inventoryService == null || item == null)
                return;

            if (!other.GetComponentInParent<PlayerController>())
                return;

            bool added = _inventoryService.AddItem(item, amount);

            if (added && destroyAfterPickup)
                Destroy(gameObject);
        }
    }
}