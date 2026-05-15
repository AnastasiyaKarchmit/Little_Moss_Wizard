using Features.Gameplay.CharacterController.Contracts;
using Features.Gameplay.Inventory.Configs;
using Features.Gameplay.Inventory.Contracts;
using UnityEngine;
using VContainer;

namespace Features.Gameplay.CharacterController.Runtime
{
    [DisallowMultipleComponent]
    public sealed class PlayerCollisionController2D : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerController player;
        [SerializeField] private PlayerDamageReceiver2D damageReceiver;
        
        private IInventoryService _inventoryService;

        [Inject]
        public void Construct(IInventoryService inventoryService)
            => _inventoryService = inventoryService;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryHandleDamageSource(other);
            // TryEnterInteraction(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            TryHandleDamageSource(other);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            //TryExitInteraction(other);
        }

        public bool TryHandleCollectable(InventoryItemDefinition item, int amount)
        {
            return _inventoryService.AddItem(item, amount);
        }

        private void TryHandleDamageSource(Collider2D other)
        {
            IDamageSource2D damageSource = GetComponentFromCollider<IDamageSource2D>(other);

            if (damageSource == null)
                return;

            if (damageReceiver == null)
                return;

            damageReceiver.TryReceiveDamage(damageSource, transform.position);
        }

        private static T GetComponentFromCollider<T>(Collider2D collider)
            where T : class
        {
            if (collider.TryGetComponent(out T component))
                return component;

            return collider.GetComponentInParent<T>();
        }

        private void ResolveReferences()
        {
            if (player == null)
                player = GetComponentInParent<PlayerController>();

            if (damageReceiver == null)
                damageReceiver = GetComponentInParent<PlayerDamageReceiver2D>();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ResolveReferences();
        }
#endif
    }
}