using System;
using System.Collections.Generic;
using Core.UI.Views;
using Features.Gameplay.Inventory.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.Gameplay.Inventory.UI
{
    public sealed class InventoryView : BaseView
    {
        [Header("Slots")]
        [SerializeField] private List<InventorySlotView> slots = new();

        [Header("Details")]
        [SerializeField] private TMP_Text itemNameText;
        [SerializeField] private TMP_Text itemDescriptionText;

        [Header("Buttons")]
        [SerializeField] private Button useButton;
        [SerializeField] private Button closeButton;

        private Action<int> _slotClicked;
        private Action _useClicked;
        private Action _closeClicked;

        public void Initialize(
            Action<int> slotClicked,
            Action useClicked,
            Action closeClicked)
        {
            _slotClicked = slotClicked;
            _useClicked = useClicked;
            _closeClicked = closeClicked;

            for (int i = 0; i < slots.Count; i++)
                slots[i].Initialize(i, OnSlotClicked);

            if (useButton != null)
            {
                useButton.onClick.RemoveListener(OnUseClicked);
                useButton.onClick.AddListener(OnUseClicked);
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(OnCloseClicked);
                closeButton.onClick.AddListener(OnCloseClicked);
            }
        }

        public void SetSlots(
            IReadOnlyList<InventorySlotData> inventorySlots,
            int selectedIndex)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                InventorySlotData slot = i < inventorySlots.Count
                    ? inventorySlots[i]
                    : InventorySlotData.Empty;

                slots[i].SetSlot(slot);
                slots[i].SetSelected(i == selectedIndex);
            }

            SetSelectedItemDetails(
                selectedIndex >= 0 && selectedIndex < inventorySlots.Count
                    ? inventorySlots[selectedIndex]
                    : InventorySlotData.Empty);
        }

        private void SetSelectedItemDetails(InventorySlotData slot)
        {
            bool hasItem = !slot.IsEmpty;

            if (itemNameText != null)
                itemNameText.text = hasItem ? slot.Item.DisplayName : string.Empty;

            if (itemDescriptionText != null)
                itemDescriptionText.text = hasItem ? slot.Item.Description : string.Empty;

            if (useButton != null)
                useButton.interactable = hasItem && slot.Item.CanUse;
        }

        private void OnSlotClicked(int index)
        {
            _slotClicked?.Invoke(index);
        }

        private void OnUseClicked()
        {
            _useClicked?.Invoke();
        }

        private void OnCloseClicked()
        {
            _closeClicked?.Invoke();
        }

        private void OnDestroy()
        {
            if (useButton != null)
                useButton.onClick.RemoveListener(OnUseClicked);

            if (closeButton != null)
                closeButton.onClick.RemoveListener(OnCloseClicked);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (slots == null || slots.Count == 0)
                slots = new List<InventorySlotView>(GetComponentsInChildren<InventorySlotView>(true));
        }
#endif
    }
}