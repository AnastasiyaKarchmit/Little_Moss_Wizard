using System.Threading;
using Cysharp.Threading.Tasks;

namespace Features.Gameplay.Interactions.Contracts
{
    public interface IPlayerInteractable2D
    {
        bool CanInteract { get; }

        UniTask InteractAsync(
            IPlayerInteractionContext context,
            CancellationToken token = default);
    }
}