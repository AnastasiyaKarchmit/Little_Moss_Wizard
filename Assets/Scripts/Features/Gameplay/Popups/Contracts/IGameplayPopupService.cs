using System;
using Features.Gameplay.Popups.Data;

namespace Features.Gameplay.Popups.Contracts
{
    public interface IGameplayPopupService
    {
        event Action<GameplayPopupRequest> PopupRequested;

        void Show(GameplayPopupRequest request);
    }
}