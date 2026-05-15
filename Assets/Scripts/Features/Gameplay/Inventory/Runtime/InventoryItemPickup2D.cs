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

        private PlayerCollisionController2D _playerCollisionController;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (item == null)
                return;

            var controller = other.GetComponent<PlayerCollisionController2D>();
            
            if (controller == null)
                return;

            bool added = controller.TryHandleCollectable(item, amount);

            if (added && destroyAfterPickup)
                Destroy(gameObject);
        }
    }
}