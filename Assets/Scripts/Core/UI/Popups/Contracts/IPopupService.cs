using System;
using Core.UI.Popups.Data;

namespace Core.UI.Popups.Contracts
{
    public interface IPopupService
    {
        event Action<PopupRequest> PopupRequested;

        void Show(PopupRequest request);
    }
}