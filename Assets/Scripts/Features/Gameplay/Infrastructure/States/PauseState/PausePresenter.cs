using System;
using System.Threading;
using Core.Audio.Contracts;
using Core.Input.Contracts;
using Core.Input.Runtime;
using Core.Patterns.MVP;
using Core.UI.Windows.Contracts;
using Core.UI.Windows.Data;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace Features.Gameplay.Infrastructure.States.PauseState
{
    public class PausePresenter : IPresenter
    {
        private readonly GameplayModel _model;
        private readonly IWindowService _windowService;
        private readonly IInputService _inputService;
        private readonly IUISoundPlayer _uiSoundPlayer;

        // Commands used by the View / input.
        private readonly ReactiveCommand<Unit> _resumeClickedCommand = new();
        private readonly ReactiveCommand<Unit> _settingsClickedCommand = new();
        private readonly ReactiveCommand<Unit> _backToMenuClickedCommand = new();

        // Commands exposed to GameplayFlowController.
        private readonly ReactiveCommand<Unit> _resumeRequestedCommand = new();
        private readonly ReactiveCommand<Unit> _settingsRequestedCommand = new();
        private readonly ReactiveCommand<Unit> _backToMenuRequestedCommand = new();

        private readonly CompositeDisposable _screenDisposables = new();
        private readonly CompositeDisposable _lifetimeDisposables = new();

        private readonly TimeSpan _inputThrottle = TimeSpan.FromMilliseconds(300);

        public Observable<Unit> ResumeRequested => _resumeRequestedCommand;
        public Observable<Unit> SettingsRequested => _settingsRequestedCommand;
        public Observable<Unit> BackToMenuRequested => _backToMenuRequestedCommand;

        private PauseView _view;

        public PausePresenter(
            GameplayModel model,
            IWindowService windowService,
            IInputService inputService,
            IUISoundPlayer uiSoundPlayer)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _windowService = windowService ?? throw new ArgumentNullException(nameof(windowService));
            _inputService = inputService ?? throw new ArgumentNullException(nameof(inputService));
            _uiSoundPlayer = uiSoundPlayer ?? throw new ArgumentNullException(nameof(uiSoundPlayer));

            SubscribeToClickCommands();
        }

        public async UniTask EnterAsync(CancellationToken token = default)
        {
            _inputService.SetMode(InputMode.Disabled);
            Time.timeScale = 0f;

            _view = await _windowService.GetOrCreateAsync<PauseView>(
                WindowId.Pause,
                token);

            token.ThrowIfCancellationRequested();

            _view.Initialize(
                _resumeClickedCommand,
                _settingsClickedCommand,
                _backToMenuClickedCommand);

            _view.ShowInstantly();

            _inputService.SetMode(InputMode.UIOnly);

            SubscribeToInput();
        }

        public async UniTask ExitAsync(CancellationToken token = default)
        {
            _screenDisposables.Clear();

            if (_view != null)
                await _view.HideAsync();

            _view = null;

            Time.timeScale = 1f;
        }

        public void HideInstantly()
        {
            _screenDisposables.Clear();
            _view?.HideInstantly();
            _view = null;
        }

        private void SubscribeToInput()
        {
            _screenDisposables.Clear();

            _inputService.UI.Cancel.Performed
                .Where(pressed => pressed)
                .ThrottleFirst(_inputThrottle)
                .Subscribe(_ => _resumeClickedCommand.Execute(Unit.Default))
                .AddTo(_screenDisposables);
        }

        private void SubscribeToClickCommands()
        {
            _resumeClickedCommand
                .Subscribe(_ =>
                {
                    _uiSoundPlayer.PlayButtonClick();
                    _resumeRequestedCommand.Execute(Unit.Default);
                })
                .AddTo(_lifetimeDisposables);

            _settingsClickedCommand
                .Subscribe(_ =>
                {
                    _uiSoundPlayer.PlayButtonClick();
                    _settingsRequestedCommand.Execute(Unit.Default);
                })
                .AddTo(_lifetimeDisposables);

            _backToMenuClickedCommand
                .Subscribe(_ =>
                {
                    _uiSoundPlayer.PlayButtonClick();
                    _backToMenuRequestedCommand.Execute(Unit.Default);
                })
                .AddTo(_lifetimeDisposables);
        }

        public void Dispose()
        {
            _screenDisposables.Dispose();
            _lifetimeDisposables.Dispose();

            _resumeClickedCommand.Dispose();
            _settingsClickedCommand.Dispose();
            _backToMenuClickedCommand.Dispose();

            _resumeRequestedCommand.Dispose();
            _settingsRequestedCommand.Dispose();
            _backToMenuRequestedCommand.Dispose();
        }
    }
}