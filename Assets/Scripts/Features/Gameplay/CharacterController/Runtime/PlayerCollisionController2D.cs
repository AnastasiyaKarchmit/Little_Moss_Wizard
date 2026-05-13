using Features.Gameplay.CharacterController.Contracts;
using UnityEngine;

namespace Features.Gameplay.CharacterController.Runtime
{
    [DisallowMultipleComponent]
    public sealed class PlayerCollisionController2D : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerController player;
        [SerializeField] private PlayerDamageReceiver2D damageReceiver;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryHandleDamageSource(other);
            // TryHandleCollectable(other);
            // TryEnterInteraction(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            TryHandleDamageSource(other);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            //TryExitInteraction(other);
        }

        private void TryHandleDamageSource(Collider2D other)
        {
            IDamageSource2D damageSource = GetComponentFromCollider<IDamageSource2D>(other);

            if (damageSource == null)
                return;

            if (damageReceiver == null)
                return;

            damageReceiver.TryReceiveDamage(damageSource, transform.position);
        }

        // private void TryHandleCollectable(Collider2D other)
        // {
        //     ICollectable2D collectable = GetComponentFromCollider<ICollectable2D>(other);
        //
        //     if (collectable == null)
        //         return;
        //
        //     if (!collectable.CanCollect)
        //         return;
        //
        //     collectable.Collect(player);
        // }
        //
        // private void TryEnterInteraction(Collider2D other)
        // {
        //     IInteractable2D interactable = GetComponentFromCollider<IInteractable2D>(other);
        //
        //     if (interactable == null)
        //         return;
        //
        //     if (!interactable.CanInteract)
        //         return;
        //
        //     interactable.EnterInteractionRange(player);
        // }
        //
        // private void TryExitInteraction(Collider2D other)
        // {
        //     IInteractable2D interactable = GetComponentFromCollider<IInteractable2D>(other);
        //
        //     if (interactable == null)
        //         return;
        //
        //     interactable.ExitInteractionRange(player);
        // }

        private static T GetComponentFromCollider<T>(Collider2D collider)
            where T : class
        {
            if (collider.TryGetComponent(out T component))
                return component;

            return collider.GetComponentInParent<T>();
        }

        private void ResolveReferences()
        {
            if (player == null)
                player = GetComponentInParent<PlayerController>();

            if (damageReceiver == null)
                damageReceiver = GetComponentInParent<PlayerDamageReceiver2D>();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ResolveReferences();
        }
#endif
    }
}