using System;
using Features.Gameplay.Inventory.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Features.Gameplay.Inventory.UI
{
    [DisallowMultipleComponent]
    public sealed class InventorySlotView : Button
    {
        [Header("References")]
        [SerializeField] private Button button;
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text amountText;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Sprite selectedSprite;
        [SerializeField] private Sprite deselectedSprite;

        private int _index;
        private bool _hasItem;
        private bool _isSelectedByInventory;

        private Action<int> _focused;
        private Action<int> _clicked;

        public void Initialize(
            int index,
            Action<int> focused,
            Action<int> clicked)
        {
            _index = index;
            _focused = focused;
            _clicked = clicked;

            onClick.RemoveListener(OnClicked);
            onClick.AddListener(OnClicked);
        }

        public void SetSlot(InventorySlotData slot)
        {
            _hasItem = !slot.IsEmpty;

            if (icon != null)
            {
                icon.enabled = _hasItem;
                icon.sprite = _hasItem ? slot.Item.Icon : null;
            }

            if (amountText != null)
            {
                bool showAmount = _hasItem && slot.Amount > 1;

                amountText.gameObject.SetActive(showAmount);
                amountText.text = showAmount ? slot.Amount.ToString() : string.Empty;
            }

            interactable = _hasItem;

            ApplyBackground(currentSelectionState);
        }

        public void SetSelected(bool selected)
        {
            _isSelectedByInventory = selected;
            ApplyBackground(currentSelectionState);
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);

            if (!IsInteractable())
                return;

            Focus();

            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(gameObject);
        }

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);

            if (!IsInteractable())
                return;

            Focus();
        }

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);
            ApplyBackground(state);
        }

        private void Focus()
        {
            if (!_hasItem)
                return;

            _focused?.Invoke(_index);
        }

        private void OnClicked()
        {
            if (!_hasItem)
                return;

            _clicked?.Invoke(_index);
        }

        private void ApplyBackground(SelectionState state)
        {
            if (backgroundImage == null)
                return;

            if (!IsInteractable())
            {
                backgroundImage.sprite = deselectedSprite;
                return;
            }

            bool visuallySelected =
                _isSelectedByInventory ||
                state == SelectionState.Highlighted ||
                state == SelectionState.Selected ||
                state == SelectionState.Pressed;

            backgroundImage.sprite = visuallySelected
                ? selectedSprite
                : deselectedSprite;
        }

        protected override void OnDestroy()
        {
            onClick.RemoveListener(OnClicked);
            base.OnDestroy();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            if (backgroundImage == null)
                backgroundImage = targetGraphic as Image;
        }
#endif
    }
}