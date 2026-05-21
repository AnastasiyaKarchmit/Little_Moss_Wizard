using Cysharp.Threading.Tasks;
using DG.Tweening;
using Features.Gameplay.CharacterController.Runtime;
using TMPro;
using UnityEngine;

namespace Features.Gameplay.Environment
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public sealed class WorldHintTrigger2D : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;

        [Header("Content")]
        [SerializeField] private string title;
        [SerializeField, TextArea] private string description;

        [Header("Animation")]
        [SerializeField] private bool showOnStart;
        [SerializeField, Min(0f)] private float fadeDuration = 0.25f;
        [SerializeField] private bool usePunchScale = true;
        [SerializeField] private Vector3 punchScale = new(0.08f, 0.08f, 0f);
        [SerializeField, Min(0f)] private float punchDuration = 0.2f;

        [Header("Trigger")]
        [SerializeField] private bool triggerOnlyOnce = true;

        private Tween _fadeTween;
        private Tween _punchTween;

        private bool _wasTriggered;
        private bool _isVisible;

        private void Awake()
        {
            ResolveReferences();
            ApplyText();

            Collider2D trigger = GetComponent<Collider2D>();
            trigger.isTrigger = true;

            if (showOnStart)
                ShowInstantly();
            else
                HideInstantly();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (triggerOnlyOnce && _wasTriggered)
                return;

            if (!other.GetComponentInParent<PlayerController>())
                return;

            _wasTriggered = true;
            ShowAsync().Forget();
        }

        public async UniTask ShowAsync()
        {
            if (_isVisible)
                return;

            _isVisible = true;

            KillTweens();

            gameObject.SetActive(true);

            if (canvasGroup != null)
            {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;

                _fadeTween = canvasGroup
                    .DOFade(1f, fadeDuration)
                    .SetUpdate(false);

                await _fadeTween.ToUniTask();
            }

            if (usePunchScale)
            {
                _punchTween = transform
                    .DOPunchScale(punchScale, punchDuration)
                    .SetUpdate(false);
            }
        }

        public void ShowInstantly()
        {
            KillTweens();

            _isVisible = true;
            gameObject.SetActive(true);

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
        }

        public void HideInstantly()
        {
            KillTweens();

            _isVisible = false;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
        }

        private void ApplyText()
        {
            if (titleText != null)
                titleText.text = title ?? string.Empty;

            if (descriptionText != null)
                descriptionText.text = description ?? string.Empty;
        }

        private void KillTweens()
        {
            if (_fadeTween != null && _fadeTween.IsActive())
                _fadeTween.Kill();

            if (_punchTween != null && _punchTween.IsActive())
                _punchTween.Kill();

            _fadeTween = null;
            _punchTween = null;
        }

        private void ResolveReferences()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponentInChildren<CanvasGroup>(true);
        }

        private void OnDestroy()
        {
            KillTweens();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ResolveReferences();
            ApplyText();

            Collider2D trigger = GetComponent<Collider2D>();

            if (trigger != null)
                trigger.isTrigger = true;
        }
#endif
    }
}