using System;
using R3;

namespace Core.GameplayCompletionService
{
    public interface IGameplayCompletionService
    {
        Observable<Unit> Completed { get; }
        bool IsCompleted { get; }

        void Complete();
        void Reset();
    }

    public sealed class GameplayCompletionService : IGameplayCompletionService, IDisposable
    {
        private readonly ReactiveCommand<Unit> _completed = new();

        private bool _isCompleted;

        public bool IsCompleted => _isCompleted;
        public Observable<Unit> Completed => _completed;

        public void Complete()
        {
            if (_isCompleted)
                return;

            _isCompleted = true;
            _completed.Execute(Unit.Default);
        }

        public void Reset()
        {
            _isCompleted = false;
        }

        public void Dispose()
        {
            _completed.Dispose();
        }
    }
}