using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Core.UI.Animations
{
    public sealed class AnimatedButton : Button
    {
        [Header("Scale")]
        [SerializeField] private float pressedScale = 0.92f;
        [SerializeField] private float selectedScale = 1.05f;
        [SerializeField] private float normalScale = 1f;

        [Header("Timing")]
        [SerializeField] private float pressDuration = 0.08f;
        [SerializeField] private float releaseDuration = 0.12f;

        [Header("Ease")]
        [SerializeField] private Ease pressEase = Ease.OutQuad;
        [SerializeField] private Ease releaseEase = Ease.OutBack;

        [SerializeField] private GameObject arrows;

        private Tween _scaleTween;
        private RectTransform _rectTransform;

        protected override void Awake()
        {
            base.Awake();
            _rectTransform = transform as RectTransform;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            KillTween();

            if (_rectTransform != null)
                _rectTransform.localScale = Vector3.one * normalScale;
            
            arrows.gameObject.SetActive(false);
        }

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);
            
            if (!IsInteractable())
                return;
            
            switch (state)
            {
                case SelectionState.Normal:
                    SetDeselected();
                    break;
                case SelectionState.Pressed:
                    ScaleTo(pressedScale, releaseDuration, releaseEase);
                    arrows.gameObject.SetActive(true);
                    break;
                case SelectionState.Highlighted:
                    SetSelected();
                    break;
                case SelectionState.Selected:
                    SetSelected();
                    break;
                case SelectionState.Disabled:
                    SetDeselected();
                    break;
            }
        }
        
        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
        
            if (!IsInteractable())
                return;
            
            EventSystem.current.SetSelectedGameObject(gameObject);
        
            ScaleTo(IsSelected(eventData) ? selectedScale : normalScale, releaseDuration, releaseEase);
        }

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);

            if (!IsInteractable())
                return;

            SetSelected();
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);

            if (!IsInteractable())
                return;
            
            SetDeselected();
        }

        public override void OnSubmit(BaseEventData eventData)
        {
            base.OnSubmit(eventData);

            if (!IsInteractable())
                return;

            PlaySubmitAnimation();
        }

        private void SetSelected()
        {
            ScaleTo(selectedScale, releaseDuration, releaseEase);
            arrows.gameObject.SetActive(true);
        }

        private void SetDeselected()
        {
            arrows.gameObject.SetActive(false);
            ScaleTo(normalScale, releaseDuration, releaseEase);
        }
        
        private void PlaySubmitAnimation()
        {
            KillTween();

            float targetScale = IsSelected(null) ? selectedScale : normalScale;

            _scaleTween = DOTween.Sequence()
                .Append(_rectTransform.DOScale(pressedScale, pressDuration).SetEase(pressEase))
                .Append(_rectTransform.DOScale(targetScale, releaseDuration).SetEase(releaseEase))
                .SetLink(gameObject);
        }

        private void ScaleTo(float scale, float duration, Ease ease)
        {
            KillTween();

            _scaleTween = _rectTransform
                .DOScale(scale, duration)
                .SetEase(ease)
                .SetLink(gameObject);
        }

        private void KillTween()
        {
            if (_scaleTween != null && _scaleTween.IsActive())
                _scaleTween.Kill();
        }

        private bool IsSelected(BaseEventData eventData)
        {
            return EventSystem.current != null &&
                   EventSystem.current.currentSelectedGameObject == gameObject;
        }
        
    }
}