using Core.Save;
using Cysharp.Threading.Tasks;
using Features.Gameplay.Collectibles.Contracts;
using Features.Gameplay.Inventory.Runtime;
using UnityEngine;
using VContainer;

namespace Features.Gameplay.Collectibles.Runtime
{
    public sealed class SceneCollectiblesController : MonoBehaviour
    {
        [SerializeField] private Transform collectiblesParent;

        private ICollectibleService _collectibleService;
        private ISaveSystem _saveSystem;

        [Inject]
        public void Construct(
            ICollectibleService collectibleService,
            ISaveSystem saveSystem)
        {
            _collectibleService = collectibleService;
            _saveSystem = saveSystem;
        }

        private async void Start()
        {
            if (_saveSystem is { IsLoaded: false })
                await _saveSystem.LoadAsync();

            if (_collectibleService == null)
                await UniTask.WaitUntil(() => _collectibleService != null);
            
            ApplyCollectedStates();
        }

        private void ApplyCollectedStates()
        {
            Transform root = collectiblesParent != null
                ? collectiblesParent
                : transform;

            InventoryItemCollectible2D[] collectibles =
                root.GetComponentsInChildren<InventoryItemCollectible2D>(true);

            foreach (InventoryItemCollectible2D collectible in collectibles)
            {
                if (collectible == null)
                    continue;

                if (_collectibleService.IsCollected(collectible.Id))
                    collectible.ApplyCollectedState();
            }
        }
    }
}