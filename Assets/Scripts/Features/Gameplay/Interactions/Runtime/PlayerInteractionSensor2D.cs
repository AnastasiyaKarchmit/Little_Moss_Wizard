using Features.Gameplay.CharacterController.Runtime;
using UnityEngine;

namespace Features.Gameplay.Interactions.Runtime
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public sealed class PlayerInteractionSensor2D : MonoBehaviour
    {
        [SerializeField] private PlayerInteractionController2D interactionController;

        private void Awake()
        {
            if (interactionController == null)
                interactionController = GetComponentInParent<PlayerInteractionController2D>();

            Collider2D sensorCollider = GetComponent<Collider2D>();
            sensorCollider.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log($"Interaction sensor entered: {other.name}");
            interactionController?.HandleTriggerEnter(other);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            interactionController?.HandleTriggerExit(other);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (interactionController == null)
                interactionController = GetComponentInParent<PlayerInteractionController2D>();

            Collider2D sensorCollider = GetComponent<Collider2D>();

            if (sensorCollider != null)
                sensorCollider.isTrigger = true;
        }
#endif
    }
}