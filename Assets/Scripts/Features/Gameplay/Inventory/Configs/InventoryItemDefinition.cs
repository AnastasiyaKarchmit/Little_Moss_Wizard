using Features.Gameplay.Inventory.Contracts;
using UnityEditor;
using UnityEngine;

namespace Features.Gameplay.Inventory.Configs
{
    [CreateAssetMenu(
        fileName = "InventoryItem",
        menuName = "Configs/Inventory/Item")]
    public sealed class InventoryItemDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField, HideInInspector] private string id;
        
        [SerializeField] private string displayName;
        [SerializeField, TextArea] private string description;

        [Header("Visuals")]
        [SerializeField] private Sprite icon;

        [Header("Stacking")]
        [SerializeField] private bool stackable = true;
        [SerializeField, Min(1)] private int maxStack = 9;

        [Header("Use")]
        [SerializeField] private bool consumeOnUse = true;
        [SerializeField] private InventoryItemUseAction useAction;

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;

        public bool Stackable => stackable;
        public int MaxStack => stackable ? maxStack : 1;

        public bool ConsumeOnUse => consumeOnUse;
        public bool CanUse => useAction != null;

        public bool CanUseItem(IInventoryItemUseContext context)
        {
            return useAction != null && useAction.CanUse(context);
        }

        public bool Use(IInventoryItemUseContext context)
        {
            return useAction != null && useAction.Use(context);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!stackable)
                maxStack = 1;

            maxStack = Mathf.Max(1, maxStack);

            AssignStableAssetId();
        }

        private void AssignStableAssetId()
        {
            string path = AssetDatabase.GetAssetPath(this);

            if (string.IsNullOrWhiteSpace(path))
                return;

            string assetGuid = AssetDatabase.AssetPathToGUID(path);

            if (string.IsNullOrWhiteSpace(assetGuid))
                return;

            if (id == assetGuid)
                return;

            id = assetGuid;
            EditorUtility.SetDirty(this);
        }
#endif
    }
}