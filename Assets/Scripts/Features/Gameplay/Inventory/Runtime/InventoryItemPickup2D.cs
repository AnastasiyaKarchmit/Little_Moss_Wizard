using Core.Save;
using Cysharp.Threading.Tasks;
using Features.Gameplay.CharacterController.Runtime;
using Features.Gameplay.Collectibles.Runtime;
using Features.Gameplay.Inventory.Configs;
using Features.Gameplay.Inventory.Contracts;
using UnityEditor;
using UnityEngine;
using VContainer;

namespace Features.Gameplay.Inventory.Runtime
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public sealed class InventoryItemCollectible2D : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField, HideInInspector] private string id;

        [Header("Item")]
        [SerializeField] private InventoryItemDefinition item;
        [SerializeField, Min(1)] private int amount = 1;

        [Header("Collected State")]
        [SerializeField] private bool deactivateWhenCollected = true;
        [SerializeField] private Collider2D[] collidersToDisable;
        [SerializeField] private Renderer[] renderersToDisable;

        public string Id => id;
        public InventoryItemDefinition Item => item;
        public int Amount => amount;

        public bool IsValid => !string.IsNullOrWhiteSpace(id) && item != null && amount > 0;

        private void Awake()
        {
            Collider2D trigger = GetComponent<Collider2D>();
            trigger.isTrigger = true;
        }

        public void ApplyCollectedState()
        {
            if (deactivateWhenCollected)
            {
                gameObject.SetActive(false);
                return;
            }

            if (collidersToDisable != null)
            {
                foreach (Collider2D targetCollider in collidersToDisable)
                {
                    if (targetCollider != null)
                        targetCollider.enabled = false;
                }
            }

            if (renderersToDisable != null)
            {
                foreach (Renderer targetRenderer in renderersToDisable)
                {
                    if (targetRenderer != null)
                        targetRenderer.enabled = false;
                }
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(id))
                GenerateNewId();

            Collider2D trigger = GetComponent<Collider2D>();

            if (trigger != null)
                trigger.isTrigger = true;

            if (collidersToDisable == null || collidersToDisable.Length == 0)
                collidersToDisable = GetComponentsInChildren<Collider2D>(true);

            if (renderersToDisable == null || renderersToDisable.Length == 0)
                renderersToDisable = GetComponentsInChildren<Renderer>(true);
        }

        [ContextMenu("Generate New Collectible Id")]
        private void GenerateNewId()
        {
            id = GUID.Generate().ToString();
            EditorUtility.SetDirty(this);
        }
#endif
    }
}