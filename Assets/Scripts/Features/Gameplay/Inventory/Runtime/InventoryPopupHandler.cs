using System;
using Core.UI.Popups.Contracts;
using Core.UI.Popups.Requests;
using Cysharp.Threading.Tasks;
using Features.Gameplay.Inventory.Configs;
using Features.Gameplay.Inventory.Contracts;
using VContainer.Unity;

namespace Features.Gameplay.Inventory.Runtime
{
    public sealed class InventoryPopupHandler : IInitializable, IDisposable
    {
        private readonly IInventoryService _inventoryService;
        private readonly IPopupService _popupService;

        public InventoryPopupHandler(
            IInventoryService inventoryService,
            IPopupService popupService)
        {
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            _popupService = popupService ?? throw new ArgumentNullException(nameof(popupService));
        }
        
        public void Initialize()
        {
            _inventoryService.ItemAdded += OnItemAdded;
        }

        public void Dispose()
        {
            _inventoryService.ItemAdded -= OnItemAdded;
        }

        private void OnItemAdded(InventoryItemDefinition item, int amount)
        {
            if (item == null)
                return;

            string amountText = amount >= 1
                ? $"x{amount}"
                : string.Empty;

            _popupService
                .ShowAsync(
                    new TimedPopupRequest(
                        item.Icon,
                        item.DisplayName,
                        string.Empty,
                        amountText,
                        2f))
                .Forget();
        }
    }
}