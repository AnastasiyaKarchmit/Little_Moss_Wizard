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

namespace Features.MainMenu.States.MainMenuState
{
    public sealed class MainMenuPresenter : IPresenter
    {
        private readonly MainMenuModel _model;
        private readonly IWindowService _windowService;
        private readonly IInputService _inputService;
        private readonly IUISoundPlayer _uiSoundPlayer;

        private readonly ReactiveCommand<Unit> _playCommand = new();
        private readonly ReactiveCommand<Unit> _settingsCommand = new();
        private readonly CompositeDisposable _disposables = new();

        private MainMenuView _view;

        public Observable<Unit> PlayRequested => _playCommand;
        public Observable<Unit> SettingsRequested => _settingsCommand;

        public MainMenuPresenter(
            MainMenuModel model,
            IWindowService windowService,
            IInputService inputService,
            IUISoundPlayer uiSoundPlayer)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _windowService = windowService ?? throw new ArgumentNullException(nameof(windowService));
            _inputService = inputService ?? throw new ArgumentNullException(nameof(inputService));
            _uiSoundPlayer = uiSoundPlayer ?? throw new ArgumentNullException(nameof(uiSoundPlayer));

            SubscribeToAudioEvents();
        }

        public async UniTask EnterAsync(CancellationToken token = default)
        {
            _inputService.SetMode(InputMode.UIOnly);
            
            _view = await _windowService.GetOrCreateAsync<MainMenuView>(
                WindowId.MainMenu,
                token);

            token.ThrowIfCancellationRequested();

            _view.Initialize(_playCommand, _settingsCommand);

            await _view.ShowAsync();
        }

        public async UniTask ExitAsync(CancellationToken token = default)
        {
            if (_view != null)
                await _view.HideAsync();

            _view = null;
        }

        public void HideInstantly()
        {
            _view?.HideInstantly();
        }

        private void SubscribeToAudioEvents()
        {
            _playCommand
                .Subscribe(_ => _uiSoundPlayer.PlayButtonClick())
                .AddTo(_disposables);

            _settingsCommand
                .Subscribe(_ => _uiSoundPlayer.PlayButtonClick())
                .AddTo(_disposables);
        }
        
        public void Dispose()
        {
            _playCommand.Dispose();
            _settingsCommand.Dispose();
            _disposables.Dispose();
        }
    }
}