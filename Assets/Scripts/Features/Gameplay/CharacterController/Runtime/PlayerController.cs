using UnityEngine;

namespace Features.Gameplay.CharacterController.Runtime
{
    [DisallowMultipleComponent]
    public sealed class PlayerController : MonoBehaviour
    {
        [Header("Core")]
        [SerializeField] private PlayerMovementController2D movement;
        [SerializeField] private PlayerMovementAnimator2D movementAnimator;
        [SerializeField] private PlayerAudioController2D audioController;

        // [Header("Gameplay")]
        // [SerializeField] private PlayerCollisionController2D collisionController;

        public PlayerMovementController2D Movement => movement;

        private bool _isGameplayActive = true;

        private void Awake()
        {
            ResolveReferences();
        }

        public void SetGameplayActive(bool active)
        {
            if (_isGameplayActive == active)
                return;

            _isGameplayActive = active;

            if (movement != null)
                movement.SetActive(active);

            if (movementAnimator != null)
                movementAnimator.enabled = active;

            if (audioController != null)
                audioController.enabled = active;

            // if (collisionController != null)
            //     collisionController.enabled = active;
        }

        public void EnableGameplay()
        {
            SetGameplayActive(true);
        }

        public void DisableGameplay()
        {
            SetGameplayActive(false);
        }

        private void ResolveReferences()
        {
            if (movement == null)
                movement = GetComponentInChildren<PlayerMovementController2D>();

            if (movementAnimator == null)
                movementAnimator = GetComponentInChildren<PlayerMovementAnimator2D>();

            if (audioController == null)
                audioController = GetComponentInChildren<PlayerAudioController2D>();

            // if (collisionController == null)
            //     collisionController = GetComponentInChildren<PlayerCollisionController2D>();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ResolveReferences();
        }
#endif
    }
}