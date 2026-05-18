using System;
using System.Threading;
using Core.UI.Windows.Contracts;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.Gameplay.Inventory.UI
{
    [DisallowMultipleComponent]
    public sealed class PickupPopupView : MonoBehaviour, IWindow
    {
        [Header("Window")]
        [SerializeField] private RectTransform rootRectTransform;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Content")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text amountText;
        [SerializeField] private TMP_Text itemNameText;

        [Header("Animation")]
        [SerializeField, Min(0f)] private float fadeInDuration = 0.15f;
        [SerializeField, Min(0f)] private float visibleDuration = 2f;
        [SerializeField, Min(0f)] private float fadeOutDuration = 0.25f;

        [Header("Punch")]
        [SerializeField] private bool usePunchScale = true;
        [SerializeField] private Vector3 punchScale = new(0.12f, 0.12f, 0f);
        [SerializeField, Min(0f)] private float punchDuration = 0.2f;
        [SerializeField, Min(1)] private int punchVibrato = 6;

        private CancellationTokenSource _animationCts;
        private Sequence _sequence;

        public RectTransform RootRectTransform => rootRectTransform;

        public bool IsActive => gameObject.activeSelf;

        public bool IsInteractable => false;

        public event Action<IWindow> Destroyed;

        private void Awake()
        {
            ResolveReferences();
            HideInstantly();
        }

        public void SetData(Sprite icon, int amount, string itemName)
        {
            if (iconImage != null)
            {
                iconImage.enabled = icon != null;
                iconImage.sprite = icon;
            }

            if (amountText != null)
                amountText.text = amount > 1 ? $"x{amount}" : string.Empty;

            if (itemNameText != null)
                itemNameText.text = itemName ?? string.Empty;
        }

        public async UniTask ShowTemporaryAsync(
            Sprite icon,
            int amount,
            string itemName,
            CancellationToken token = default)
        {
            SetData(icon, amount, itemName);

            CancelCurrentAnimation();

            _animationCts = CancellationTokenSource.CreateLinkedTokenSource(token);
            CancellationToken linkedToken = _animationCts.Token;

            try
            {
                await ShowAsync();

                await UniTask.Delay(
                    TimeSpan.FromSeconds(visibleDuration),
                    DelayType.UnscaledDeltaTime,
                    PlayerLoopTiming.Update,
                    linkedToken);

                await HideAsync();
            }
            catch (OperationCanceledException)
            {
            }
        }

        public async UniTask ShowAsync()
        {
            CancelSequenceOnly();

            gameObject.SetActive(true);
            transform.localScale = Vector3.one;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            _sequence = DOTween.Sequence()
                .SetUpdate(true);

            if (canvasGroup != null)
                _sequence.Append(canvasGroup.DOFade(1f, fadeInDuration));

            if (usePunchScale)
            {
                _sequence.Join(
                    transform.DOPunchScale(
                            punchScale,
                            punchDuration,
                            punchVibrato)
                        .SetUpdate(true));
            }

            await _sequence.ToUniTask();
        }

        public async UniTask HideAsync()
        {
            CancelSequenceOnly();

            _sequence = DOTween.Sequence()
                .SetUpdate(true);

            if (canvasGroup != null)
                _sequence.Append(canvasGroup.DOFade(0f, fadeOutDuration));

            await _sequence.ToUniTask();

            gameObject.SetActive(false);
        }

        public void HideInstantly()
        {
            CancelCurrentAnimation();

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            gameObject.SetActive(false);
        }

        private void CancelCurrentAnimation()
        {
            if (_animationCts != null)
            {
                if (!_animationCts.IsCancellationRequested)
                    _animationCts.Cancel();

                _animationCts.Dispose();
                _animationCts = null;
            }

            CancelSequenceOnly();
        }

        private void CancelSequenceOnly()
        {
            if (_sequence != null && _sequence.IsActive())
                _sequence.Kill();

            _sequence = null;
        }

        private void ResolveReferences()
        {
            if (rootRectTransform == null)
                rootRectTransform = transform as RectTransform;

            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
        }

        private void OnDestroy()
        {
            CancelCurrentAnimation();
            Destroyed?.Invoke(this);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ResolveReferences();
        }
#endif
    }
}