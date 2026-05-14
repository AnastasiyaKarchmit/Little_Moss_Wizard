using System;
using System.Threading;
using Core.Input.Contracts;
using Core.Input.Runtime;
using Core.UI.Windows.Contracts;
using Core.UI.Windows.Data;
using Cysharp.Threading.Tasks;
using Features.Gameplay.Inventory.Contracts;
using Features.Gameplay.Inventory.UI;
using UnityEngine;

namespace Features.Gameplay.Inventory
{
    public sealed class InventoryPresenter : IDisposable
    {
        private readonly IInventoryService _inventoryService;
        private readonly IWindowService _windowService;
        private readonly IInputService _inputService;

        private InventoryView _view;
        private int _selectedIndex;
        private bool _isOpen;

        public bool IsOpen => _isOpen;

        public InventoryPresenter(
            IInventoryService inventoryService,
            IWindowService windowService,
            IInputService inputService)
        {
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            _windowService = windowService ?? throw new ArgumentNullException(nameof(windowService));
            _inputService = inputService ?? throw new ArgumentNullException(nameof(inputService));
        }

        public async UniTask OpenAsync(CancellationToken token = default)
        {
            if (_isOpen)
                return;

            _isOpen = true;

            _view = await _windowService.GetOrCreateAsync<InventoryView>(
                WindowId.Inventory,
                token);

            token.ThrowIfCancellationRequested();

            _view.Initialize(
                OnSlotClicked,
                OnUseClicked,
                OnCloseClicked);

            _inventoryService.Changed += RefreshView;

            ClampSelectedIndex();
            RefreshView();

            await _view.ShowAsync();

            _inputService.SetMode(InputMode.UIOnly);

            Time.timeScale = 0f;
        }

        public async UniTask CloseAsync(CancellationToken token = default)
        {
            if (!_isOpen)
                return;

            _isOpen = false;

            _inventoryService.Changed -= RefreshView;

            if (_view != null)
                await _view.HideAsync();

            _view = null;

            Time.timeScale = 1f;
            _inputService.SetMode(InputMode.Gameplay);
        }

        public async UniTask ToggleAsync(CancellationToken token = default)
        {
            if (_isOpen)
                await CloseAsync(token);
            else
                await OpenAsync(token);
        }

        private void OnSlotClicked(int index)
        {
            _selectedIndex = index;
            RefreshView();
        }

        private void OnUseClicked()
        {
            _inventoryService.UseAt(_selectedIndex);

            ClampSelectedIndex();
            RefreshView();
        }

        private void OnCloseClicked()
        {
            CloseAsync().Forget();
        }

        private void RefreshView()
        {
            if (_view == null)
                return;

            _view.SetSlots(_inventoryService.Slots, _selectedIndex);
        }

        private void ClampSelectedIndex()
        {
            if (_inventoryService.Capacity <= 0)
            {
                _selectedIndex = -1;
                return;
            }

            _selectedIndex = Mathf.Clamp(
                _selectedIndex,
                0,
                _inventoryService.Capacity - 1);
        }

        public void Dispose()
        {
            _inventoryService.Changed -= RefreshView;

            if (_view != null)
                _view.HideInstantly();

            _view = null;
            _isOpen = false;
        }
    }
}