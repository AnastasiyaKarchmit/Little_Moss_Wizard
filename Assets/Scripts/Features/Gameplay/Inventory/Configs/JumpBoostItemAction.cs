using Features.Gameplay.Inventory.Contracts;
using UnityEngine;

namespace Features.Gameplay.Inventory.Configs
{
    [CreateAssetMenu(
        fileName = "JumpBoostItemAction",
        menuName = "Configs/Inventory/Actions/Jump Boost")]
    public sealed class JumpBoostItemAction : InventoryItemUseAction
    {
        [Header("Jump Boost")]
        [Tooltip("1.35 means full jump height becomes 35% higher.")]
        [SerializeField, Range(1f, 3f)] private float jumpHeightMultiplier = 1.35f;

        [Tooltip("Boost duration in seconds.")]
        [SerializeField, Min(0.1f)] private float duration = 10f;

        public override bool CanUse(IInventoryItemUseContext context)
        {
            if (context?.PlayerBoosts == null)
                return false;

            if (context.PlayerHealth != null && !context.PlayerHealth.IsAlive)
                return false;

            return true;
        }

        public override bool Use(IInventoryItemUseContext context)
        {
            if (!CanUse(context))
                return false;

            context.PlayerBoosts.ApplyJumpBoost(
                jumpHeightMultiplier,
                duration);

            return true;
        }
    }
}