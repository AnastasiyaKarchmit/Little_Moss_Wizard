using Core.UI.Popups.Contracts;
using UnityEngine;

namespace Core.UI.Popups.Requests
{
    public sealed class MessagePopupRequest : PopupRequest<PopupClosed>
    {
        public Sprite Icon { get; }
        public string Title { get; }
        public string Message { get; }
        public string CloseButtonText { get; }

        public MessagePopupRequest(
            Sprite icon,
            string title,
            string message,
            string closeButtonText = "OK")
        {
            Icon = icon;
            Title = title;
            Message = message;
            CloseButtonText = closeButtonText;
        }
    }
}