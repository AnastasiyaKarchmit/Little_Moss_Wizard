using System;
using System.Threading;
using Core.Input.Contracts;
using Core.Input.Runtime;
using Core.Patterns.MVP;
using Core.UI.Windows.Contracts;
using Core.UI.Windows.Data;
using Cysharp.Threading.Tasks;
using Features.Gameplay.Inventory.Contracts;
using R3;
using UnityEngine;

namespace Features.Gameplay.Infrastructure.States.InventoryState
{
    public sealed class InventoryPresenter : IPresenter
    {
        private readonly IInventoryService _inventoryService;
        private readonly IWindowService _windowService;
        private readonly IInputService _inputService;

        private readonly CompositeDisposable _screenDisposables = new();
        private readonly ReactiveCommand<Unit> _closeCommand = new();

        private readonly TimeSpan _inputThrottle = TimeSpan.FromMilliseconds(300);

        private InventoryView _view;
        private int _selectedIndex;

        public Observable<Unit> CloseRequested => _closeCommand;

        public InventoryPresenter(
            IInventoryService inventoryService,
            IWindowService windowService,
            IInputService inputService)
        {
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            _windowService = windowService ?? throw new ArgumentNullException(nameof(windowService));
            _inputService = inputService ?? throw new ArgumentNullException(nameof(inputService));
        }

        public async UniTask EnterAsync(CancellationToken token = default)
        {
            _view = await _windowService.GetOrCreateAsync<InventoryView>(
                WindowId.Inventory,
                token);

            token.ThrowIfCancellationRequested();

            _view.Initialize(
                OnSlotFocused,
                OnSlotClicked);

            _inventoryService.Changed += RefreshView;

            ClampSelectedIndex();
            RefreshView();

            await _view.ShowAsync();

            SubscribeToInput();

            _inputService.SetMode(InputMode.UIOnly);
            Time.timeScale = 0f;
        }

        public async UniTask ExitAsync(CancellationToken token = default)
        {
            _screenDisposables.Clear();
            _inventoryService.Changed -= RefreshView;

            if (_view != null)
                await _view.HideAsync();

            _view = null;
        }

        public void HideInstantly()
        {
            _screenDisposables.Clear();
            _inventoryService.Changed -= RefreshView;

            _view?.HideInstantly();
            _view = null;
        }

        private void SubscribeToInput()
        {
            _screenDisposables.Clear();

            _inputService.UI.Cancel.Performed
                .Where(pressed => pressed)
                .ThrottleFirst(_inputThrottle)
                .Subscribe(_ => _closeCommand.Execute(Unit.Default))
                .AddTo(_screenDisposables);
        }
        
        private void OnSlotFocused(int index)
        {
            _selectedIndex = index;
            RefreshView();
        }

        private void OnSlotClicked(int index)
        {
            _selectedIndex = index;

            bool used = _inventoryService.UseAt(_selectedIndex);

            ClampSelectedIndex();
            RefreshView();
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
            _screenDisposables.Dispose();
            _inventoryService.Changed -= RefreshView;
            _closeCommand.Dispose();

            if (_view != null)
                _view.HideInstantly();

            _view = null;
        }
    }
}