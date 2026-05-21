using System;
using System.Threading;
using Core.UI.Popups.Requests;
using Cysharp.Threading.Tasks;
using Features.Gameplay.Interactions.Contracts;
using Features.Gameplay.Inventory.Configs;
using UnityEngine;

namespace Features.Gameplay.Interactions.Runtime
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public sealed class EndGameInteractable2D : MonoBehaviour, IPlayerInteractable2D
    {
        [Serializable]
        private struct RequiredItem
        {
            public InventoryItemDefinition Item;

            [Min(1)]
            public int Amount;
        }

        [Header("Requirements")]
        [SerializeField] private RequiredItem[] requiredItems;

        [Header("Success Popup")]
        [SerializeField] private string successTitle = "Potion completed!";
        [SerializeField, TextArea] private string successMessage =
            "You gathered all ingredients and brewed the final potion. The demo is complete.";

        [Header("Fail Popup")]
        [SerializeField] private string failTitle = "Missing ingredients";
        [SerializeField, TextArea] private string failMessage =
            "You still need to collect all required ingredients before brewing the potion.";

        [Header("Settings")]
        [SerializeField] private bool disableAfterSuccessfulInteraction = true;

        private bool _completed;

        public bool CanInteract => !_completed;

        private void Awake()
        {
            Collider2D trigger = GetComponent<Collider2D>();
            trigger.isTrigger = true;
        }

        public async UniTask InteractAsync(
            IPlayerInteractionContext context,
            CancellationToken token = default)
        {
            if (!CanInteract)
                return;

            bool hasAllRequiredItems = HasAllRequiredItems(context);

            if (!hasAllRequiredItems)
            {
                await context.PopupService.ShowAsync(
                    new MessagePopupRequest(
                        failTitle,
                        failMessage,
                        "OK"),
                    token);

                return;
            }

            _completed = true;

            await context.PopupService.ShowAsync(
                new MessagePopupRequest(
                    successTitle,
                    successMessage,
                    "Finish"),
                token);

            if (disableAfterSuccessfulInteraction)
                gameObject.SetActive(false);
        }

        private bool HasAllRequiredItems(IPlayerInteractionContext context)
        {
            if (context.InventoryService == null)
                return false;

            if (requiredItems == null || requiredItems.Length == 0)
                return true;

            for (int i = 0; i < requiredItems.Length; i++)
            {
                RequiredItem requirement = requiredItems[i];

                if (requirement.Item == null)
                    return false;

                if (!context.InventoryService.HasItem(
                        requirement.Item,
                        requirement.Amount))
                {
                    return false;
                }
            }

            return true;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            Collider2D trigger = GetComponent<Collider2D>();

            if (trigger != null)
                trigger.isTrigger = true;
        }
#endif
    }
}