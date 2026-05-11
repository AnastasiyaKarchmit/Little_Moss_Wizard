using System;
using System.Threading;
using Core.Audio.Contracts;
using Cysharp.Threading.Tasks;

namespace Features.Gameplay.Infrastructure
{
    public class GameplayAudioController
    {
        private readonly IAudioService _audioService;
        private readonly IAudioDatabase _audioDatabase;

        public GameplayAudioController(
            IAudioService audioService,
            IAudioDatabase audioDatabase)
        {
            _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
            _audioDatabase = audioDatabase ?? throw new ArgumentNullException(nameof(audioDatabase));
        }

        public UniTask EnterAsync(CancellationToken token)
        {
            return _audioService.PlayMusicAsync(
                _audioDatabase.Music.Gameplay,
                restartIfSame: false,
                token);
        }

        public UniTask ExitAsync(CancellationToken token)
        {
            return _audioService.StopMusicAsync(token);
        }

        public void Dispose()
        {
        }
    }
}