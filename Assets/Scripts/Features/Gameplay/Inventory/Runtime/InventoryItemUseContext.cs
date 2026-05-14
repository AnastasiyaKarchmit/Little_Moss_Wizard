using System;
using Features.Gameplay.CharacterController.Contracts;
using Features.Gameplay.Inventory.Contracts;

namespace Features.Gameplay.Inventory.Runtime
{
    public sealed class InventoryItemUseContext : IInventoryItemUseContext
    {
        public IPlayerHealth PlayerHealth { get; }
        public IPlayerBoostController PlayerBoosts { get; }

        public InventoryItemUseContext(
            IPlayerHealth playerHealth,
            IPlayerBoostController playerBoosts)
        {
            PlayerHealth = playerHealth ?? throw new ArgumentNullException(nameof(playerHealth));
            PlayerBoosts = playerBoosts ?? throw new ArgumentNullException(nameof(playerBoosts));
        }
    }
}