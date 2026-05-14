using System;
using System.Threading;
using Core.Input.Contracts;
using Core.Input.Runtime;
using Core.Patterns.MVP;
using Core.UI.Windows.Contracts;
using Core.UI.Windows.Data;
using Cysharp.Threading.Tasks;
using Features.Gameplay.CharacterController.Contracts;
using R3;
using UnityEngine;

namespace Features.Gameplay.Infrastructure.States.GameplayState
{
    public class GameplayPresenter : IPresenter
    {
        private readonly GameplayModel _model;
        private readonly IWindowService _windowService;
        private readonly IInputService _inputService;
        private readonly IPlayerHealth _playerHealth;
        
        private readonly CompositeDisposable _screenDisposables = new();
        
        private readonly ReactiveCommand<Unit> _pauseCommand = new();
        
        private readonly TimeSpan _inputThrottle = TimeSpan.FromMilliseconds(300);
        
        private GameplayView _view;
        private bool _isSubscribedToHealth;
        
        public Observable<Unit> PauseRequested => _pauseCommand;
        
        public GameplayPresenter(
            GameplayModel model,
            IWindowService windowService,
            IInputService inputService, IPlayerHealth playerHealth)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _windowService = windowService ?? throw new ArgumentNullException(nameof(windowService));
            _inputService = inputService ?? throw new ArgumentNullException(nameof(inputService));
            _playerHealth = playerHealth ?? throw new ArgumentNullException(nameof(playerHealth));
        }

        public async UniTask EnterAsync(CancellationToken token = default)
        {

            _view = await _windowService.GetOrCreateAsync<GameplayView>(
                WindowId.GameplayHud,
                token);

            token.ThrowIfCancellationRequested();

            _view.Initialize(
                _playerHealth.CurrentHealth,
                _playerHealth.MaxHealth);

            await _view.ShowAsync();

            SubscribeToInput();
            SubscribeToHealth();
            
            _inputService.SetMode(InputMode.Gameplay);
            
            Time.timeScale = 1;
        }

        public UniTask ExitAsync(CancellationToken token = default)
        {
            _screenDisposables.Clear();
            UnsubscribeFromHealth();
            
            if (_view != null)
                _view.HideInstantly();

            _view = null;
            
            return UniTask.CompletedTask;
        }

        public void HideInstantly()
        {
            _screenDisposables.Clear();
            _view?.HideInstantly();
        }
        
        private void SubscribeToInput()
        {
            _screenDisposables.Clear();

            _inputService.UI.Cancel.Performed
                .Where(pressed => pressed)
                .ThrottleFirst(_inputThrottle)
                .Subscribe(_ => _pauseCommand.Execute(Unit.Default))
                .AddTo(_screenDisposables);
        }
        
        private void SubscribeToHealth()
        {
            if (_isSubscribedToHealth)
                return;

            _playerHealth.HealthChanged += OnHealthChanged;
            _playerHealth.Damaged += OnDamaged;
            _playerHealth.Died += OnDied;

            _isSubscribedToHealth = true;
        }

        private void UnsubscribeFromHealth()
        {
            if (!_isSubscribedToHealth)
                return;

            _playerHealth.HealthChanged -= OnHealthChanged;
            _playerHealth.Damaged -= OnDamaged;
            _playerHealth.Died -= OnDied;

            _isSubscribedToHealth = false;
        }

        private void OnHealthChanged(int currentHealth, int maxHealth)
        {
            _view?.SetHealth(currentHealth, maxHealth);
        }

        private void OnDamaged(int damageAmount)
        {
            _view?.SetHealth(
                _playerHealth.CurrentHealth,
                _playerHealth.MaxHealth);
        }

        private void OnDied()
        {
            _view?.SetHealth(0, _playerHealth.MaxHealth);
        }

        public void Dispose()
        {
            _screenDisposables.Dispose();
            _pauseCommand.Dispose();
        }
    }
}