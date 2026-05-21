using Core.Save;
using Cysharp.Threading.Tasks;
using Features.Gameplay.CharacterController.Contracts;
using Features.Gameplay.Collectibles.Contracts;
using Features.Gameplay.Inventory.Configs;
using Features.Gameplay.Inventory.Contracts;
using Features.Gameplay.Inventory.Runtime;
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
        
        [Header("Collectibles")]
        [SerializeField] private bool saveImmediatelyAfterCollect = true;

        private IInventoryService _inventoryService;
        private ICollectibleService _collectibleService;
        private ISaveSystem _saveSystem;

        [Inject]
        public void Construct(
            IInventoryService inventoryService,
            ICollectibleService collectibleService,
            ISaveSystem saveSystem)
        {
            _inventoryService = inventoryService;
            _collectibleService = collectibleService;
            _saveSystem = saveSystem;
        }

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryHandleDamageSource(other);
            TryHandleCollectible(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            TryHandleDamageSource(other);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            //TryExitInteraction(other);
        }

        private void TryHandleCollectible(Collider2D other)
        {
            InventoryItemCollectible2D collectible =
                GetComponentFromCollider<InventoryItemCollectible2D>(other);

            if (collectible == null)
                return;

            if (!collectible.IsValid)
                return;

            if (_collectibleService.IsCollected(collectible.Id))
            {
                collectible.ApplyCollectedState();
                return;
            }

            bool addedToInventory = _inventoryService.AddItem(
                collectible.Item,
                collectible.Amount);

            if (!addedToInventory)
                return;

            _collectibleService.MarkCollected(collectible.Id);
            collectible.ApplyCollectedState();

            if (saveImmediatelyAfterCollect)
                _saveSystem.SaveAsync().Forget();
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