using Features.Gameplay.Inventory.Contracts;
using UnityEngine;

namespace Features.Gameplay.Inventory.Configs
{
    public abstract class InventoryItemUseAction : ScriptableObject
    {
        public virtual bool CanUse(IInventoryItemUseContext context)
        {
            return true;
        }

        public abstract bool Use(IInventoryItemUseContext context);
    }
}