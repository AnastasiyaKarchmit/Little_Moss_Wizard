using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Features.Gameplay.CharacterController.UI
{
    [DisallowMultipleComponent]
    public sealed class PlayerHealthHudView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image emptyHealthBarBackground;
        [SerializeField] private List<Image> filledHearts = new();

        [Header("Settings")]
        [SerializeField] private bool hideHeartsAboveMaxHealth = true;

        public void SetHealth(int currentHealth, int maxHealth)
        {
            maxHealth = Mathf.Max(0, maxHealth);
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

            for (int i = 0; i < filledHearts.Count; i++)
            {
                Image heart = filledHearts[i].GetComponent<Image>();

                if (heart == null)
                    continue;

                bool isInsideMaxHealth = i < maxHealth;
                bool isFilled = i < currentHealth;

                if (hideHeartsAboveMaxHealth)
                {
                    heart.gameObject.SetActive(isInsideMaxHealth && isFilled);
                }
                else
                {
                    heart.gameObject.SetActive(isFilled);
                }
            }
        }

        //TODO: change this to alpha for optimization
        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            filledHearts.RemoveAll(x => x == null);
        }
#endif
    }
}