using Core.GameplayCompletionService;
using Features.Gameplay.Infrastructure;

namespace Features.Gameplay.Interactions.Contracts
{
    public interface IEndgameInteractionContext : IPlayerInteractionContext
    {
        public IGameplayCompletionService GameplayCompletionService { get; }
    }
}