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
        [SerializeField] private PlayerBoostController boostController;
        
        [Header("Health")]
        [SerializeField] private PlayerHealth health;
        [SerializeField] private PlayerDamageReceiver2D damageReceiver;

        [Header("Gameplay")]
        [SerializeField] private PlayerCollisionController2D collisionController;
        [SerializeField] private PlayerInteractionController2D interactionController;

        private bool _isGameplayActive = true;

        private void Awake()
        {
            ResolveReferences();

            if (health != null) 
                health.Died += OnDied;
        }
        
        private void OnDestroy()
        {
            if (health != null) 
                health.Died -= OnDied;
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

            if (collisionController != null)
                collisionController.enabled = active;
            
            if (boostController != null)
                boostController.enabled = active;
            
            if (interactionController != null)
                interactionController.enabled = active;
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

            if (collisionController == null)
                collisionController = GetComponentInChildren<PlayerCollisionController2D>();
            
            if (boostController == null)
                boostController = GetComponentInChildren<PlayerBoostController>();
            
            if (interactionController == null)
                interactionController = GetComponentInChildren<PlayerInteractionController2D>();
        }
        
        private void OnDied()
        {
            DisableGameplay();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ResolveReferences();
        }
#endif
    }
}