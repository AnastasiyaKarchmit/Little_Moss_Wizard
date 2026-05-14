using Features.Gameplay.Inventory.Contracts;
using UnityEngine;

namespace Features.Gameplay.Inventory.Configs
{
    [CreateAssetMenu(
        fileName = "HealPlayerItemAction",
        menuName = "Configs/Inventory/Actions/Heal Player")]
    public sealed class HealPlayerItemAction : InventoryItemUseAction
    {
        [SerializeField, Min(1)] private int healAmount = 1;

        public override bool CanUse(IInventoryItemUseContext context)
        {
            if (context?.PlayerHealth == null)
                return false;

            return context.PlayerHealth.IsAlive &&
                   context.PlayerHealth.CurrentHealth < context.PlayerHealth.MaxHealth;
        }

        public override bool Use(IInventoryItemUseContext context)
        {
            if (!CanUse(context))
                return false;

            context.PlayerHealth.Heal(healAmount);
            return true;
        }
    }
}