using UnityEngine;

namespace Features.Gameplay.Popups.Data
{
    public readonly struct GameplayPopupRequest
    {
        public readonly Sprite Icon;
        public readonly string Title;
        public readonly string AmountText;
        public readonly float Duration;

        public GameplayPopupRequest(
            Sprite icon,
            string title,
            string amountText,
            float duration = 2f)
        {
            Icon = icon;
            Title = title;
            AmountText = amountText;
            Duration = duration;
        }
    }
}