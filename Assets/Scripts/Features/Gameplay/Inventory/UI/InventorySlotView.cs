using System;
using Features.Gameplay.Inventory.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.Gameplay.Inventory.UI
{
    [DisallowMultipleComponent]
    public sealed class InventorySlotView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Button button;
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text amountText;
        [SerializeField] private GameObject selection;

        private int _index;
        private Action<int> _clicked;

        public void Initialize(int index, Action<int> clicked)
        {
            _index = index;
            _clicked = clicked;

            if (button != null)
            {
                button.onClick.RemoveListener(OnClicked);
                button.onClick.AddListener(OnClicked);
            }
        }

        public void SetSlot(InventorySlotData slot)
        {
            bool hasItem = !slot.IsEmpty;

            if (icon != null)
            {
                icon.enabled = hasItem;
                icon.sprite = hasItem ? slot.Item.Icon : null;
            }

            if (amountText != null)
            {
                bool showAmount = hasItem && slot.Amount > 1;
                amountText.gameObject.SetActive(showAmount);
                amountText.text = showAmount ? slot.Amount.ToString() : string.Empty;
            }
        }

        public void SetSelected(bool selected)
        {
            if (selection != null)
                selection.SetActive(selected);
        }

        private void OnClicked()
        {
            _clicked?.Invoke(_index);
        }

        private void OnDestroy()
        {
            if (button != null)
                button.onClick.RemoveListener(OnClicked);
        }
    }
}