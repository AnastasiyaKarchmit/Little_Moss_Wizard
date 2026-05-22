using Core.GameplayCompletionService;
using Core.UI.Popups.Contracts;
using Features.Gameplay.CharacterController.Runtime;
using Features.Gameplay.Infrastructure;
using Features.Gameplay.Interactions.Contracts;
using Features.Gameplay.Inventory.Contracts;

namespace Features.Gameplay.Interactions.Runtime
{
    public class EndgamePlayerInteractionContext : IEndgameInteractionContext
    {
        public PlayerController Player { get; }
        public IInventoryService InventoryService { get; }
        public IPopupService PopupService { get; }
        public IGameplayCompletionService GameplayCompletionService { get; }

        public EndgamePlayerInteractionContext(
            PlayerController player,
            IInventoryService inventoryService,
            IPopupService popupService, 
            IGameplayCompletionService gameplayCompletionService)
        {
            Player = player;
            InventoryService = inventoryService;
            PopupService = popupService;
            GameplayCompletionService = gameplayCompletionService;
        }
    }
}