using Core.UI.Popups.Contracts;
using Features.Gameplay.CharacterController.Runtime;
using Features.Gameplay.Inventory.Contracts;

namespace Features.Gameplay.Interactions.Contracts
{
    public interface IPlayerInteractionContext
    {
        public PlayerController Player { get; }
        
        public IInventoryService  InventoryService { get; }
        
        public IPopupService  PopupService { get; }
    }
}