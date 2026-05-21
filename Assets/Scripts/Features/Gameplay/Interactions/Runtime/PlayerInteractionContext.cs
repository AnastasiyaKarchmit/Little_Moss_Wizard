using Core.UI.Popups.Contracts;
using Features.Gameplay.CharacterController.Runtime;
using Features.Gameplay.Interactions.Contracts;
using Features.Gameplay.Inventory.Contracts;

namespace Features.Gameplay.Interactions.Runtime
{
    public sealed class PlayerInteractionContext : IPlayerInteractionContext
    {
        public PlayerController Player { get; }
        public IInventoryService InventoryService { get; }
        public IPopupService PopupService { get; }

        public PlayerInteractionContext(
            PlayerController player,
            IInventoryService inventoryService,
            IPopupService popupService)
        {
            Player = player;
            InventoryService = inventoryService;
            PopupService = popupService;
        }
    }
}