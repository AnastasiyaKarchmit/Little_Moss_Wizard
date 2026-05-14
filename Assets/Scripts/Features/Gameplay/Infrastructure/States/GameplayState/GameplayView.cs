using Core.UI.Views;
using Features.Gameplay.CharacterController.UI;
using UnityEngine;

namespace Features.Gameplay.Infrastructure.States.GameplayState
{
    public class GameplayView : BaseView
    {
        [Header("HUD")]
        [SerializeField] private PlayerHealthHudView healthHud;

        public void Initialize(int currentHealth, int maxHealth)
        {
            SetHealth(currentHealth, maxHealth);
        }

        public void SetHealth(int currentHealth, int maxHealth)
        {
            if (healthHud == null)
                return;

            healthHud.SetHealth(currentHealth, maxHealth);
        }

        public void SetHealthVisible(bool visible)
        {
            if (healthHud == null)
                return;

            healthHud.SetVisible(visible);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (healthHud == null)
                healthHud = GetComponentInChildren<PlayerHealthHudView>(true);
        }
#endif
    }
}