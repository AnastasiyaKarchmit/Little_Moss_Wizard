using System.Threading;
using Core.SceneManagement.Loading.Contracts;
using Core.UI.Animations;
using Core.UI.Windows.Components;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.SceneManagement.Loading.Runtime
{
    public class LoadingScreen : BaseWindow, ILoadingScreen
    {
        [Header("References")]
        [SerializeField] private LoadingProgressIcon[] progressIcons;
        [SerializeField] private TMP_Text progressText;

        public override void ShowInstantly()
        {
            base.ShowInstantly();

            SetProgress(0f);
        }

        public override void HideInstantly()
        {
            Destroy(gameObject);
        }

        public override async UniTask HideAsync()
        {
            await base.HideAsync();
            Destroy(gameObject);
        }

        public void SetProgress(float progress)
        {
            progress = Mathf.Clamp01(progress);

            UpdateIcons(progress);
            UpdateText(progress);
        }

        private void UpdateIcons(float progress)
        {
            if (progressIcons == null || progressIcons.Length == 0)
                return;

            float scaledProgress = progress * progressIcons.Length;

            for (int i = 0; i < progressIcons.Length; i++)
            {
                if (progressIcons[i] == null)
                    continue;

                float iconProgress = Mathf.Clamp01(scaledProgress - i);
                progressIcons[i].SetProgress(iconProgress);
            }
        }

        private void UpdateText(float progress)
        {
            if (progressText == null)
                return;

            int percent = Mathf.RoundToInt(progress * 100f);
            progressText.text = $"{percent}%";
        }
    }
}