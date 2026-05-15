using System;
using System.Collections.Generic;
using Core.UI.Views;
using Features.Gameplay.Inventory.Data;
using Features.Gameplay.Inventory.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.Gameplay.Infrastructure.States.InventoryState
{
    public sealed class InventoryView : BaseView
    {
        [Header("Slots")]
        [SerializeField] private List<InventorySlotView> slots = new();

        [Header("Details")]
        [SerializeField] private TMP_Text itemNameText;
        [SerializeField] private TMP_Text itemDescriptionText;

        private Action<int> _slotFocused;
        private Action<int> _slotClicked;

        public void Initialize(
            Action<int> slotFocused,
            Action<int> slotClicked)
        {
            _slotFocused = slotFocused;
            _slotClicked = slotClicked;

            for (int i = 0; i < slots.Count; i++)
            {
                slots[i].Initialize(
                    i,
                    OnSlotFocused,
                    OnSlotClicked);
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

            InventorySlotData selectedSlot =
                selectedIndex >= 0 && selectedIndex < inventorySlots.Count
                    ? inventorySlots[selectedIndex]
                    : InventorySlotData.Empty;

            SetSelectedItemDetails(selectedSlot);
        }

        public void ClearDetails()
        {
            if (itemNameText != null)
                itemNameText.text = string.Empty;

            if (itemDescriptionText != null)
                itemDescriptionText.text = string.Empty;
        }

        private void SetSelectedItemDetails(InventorySlotData slot)
        {
            bool hasItem = !slot.IsEmpty;

            if (itemNameText != null)
                itemNameText.text = hasItem ? slot.Item.DisplayName : string.Empty;

            if (itemDescriptionText != null)
                itemDescriptionText.text = hasItem ? slot.Item.Description : string.Empty;
        }

        private void OnSlotFocused(int index)
        {
            _slotFocused?.Invoke(index);
        }

        private void OnSlotClicked(int index)
        {
            _slotClicked?.Invoke(index);
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