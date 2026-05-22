using System;
using Core.GameplayCompletionService;
using Core.Patterns.MVP;
using Core.Save;
using Cysharp.Threading.Tasks;
using Features.Gameplay.Infrastructure;

namespace Features.MainMenu
{
    public sealed class MainMenuModel : IModel
    {
        private readonly ISaveSystem _saveSystem;
        private readonly IGameplayCompletionService _gameplayCompletionService;

        public MainMenuModel(
            ISaveSystem saveSystem,
            IGameplayCompletionService gameplayCompletionService)
        {
            _saveSystem = saveSystem ?? throw new ArgumentNullException(nameof(saveSystem));
            _gameplayCompletionService = gameplayCompletionService ?? throw new ArgumentNullException(nameof(gameplayCompletionService));
        }

        public async UniTask EnsureSaveLoadedAsync()
        {
            if (!_saveSystem.IsLoaded)
                await _saveSystem.LoadAsync();
            
            if (_gameplayCompletionService == null)
                await UniTask.WaitUntil(() => _gameplayCompletionService != null);

            if (_gameplayCompletionService is { IsCompleted: true })
            {
                await ResetProgressAsync();
                _gameplayCompletionService.Reset();
            }
        }

        public bool HasPreviousPlaySession()
        {
            PersistentData data = _saveSystem.Data;

            if (data == null)
                return false;

            return data.Gameplay.Checkpoint != null &&
                   data.Gameplay.Checkpoint.HasCheckpoint;
        }

        public UniTask ResetProgressAsync()
        {
            return _saveSystem.ResetAsync();
        }

        public void Dispose()
        {
        }
    }
}