using System;
using Core.Input.Contracts;
using Core.UI.Popups.Contracts;
using Cysharp.Threading.Tasks;
using Features.Gameplay.Interactions.Contracts;
using Features.Gameplay.Interactions.Runtime;
using Features.Gameplay.Inventory.Contracts;
using R3;
using UnityEngine;
using VContainer;

namespace Features.Gameplay.CharacterController.Runtime
{
    [DisallowMultipleComponent]
    public sealed class PlayerInteractionController2D : MonoBehaviour
    {
        private IPlayerInteractionContext _context;
        private IInputService _inputService;
        private IInventoryService _inventoryService;
        private IPopupService _popupService;

        private readonly CompositeDisposable _disposables = new();

        private IPlayerInteractable2D _currentInteractable;
        private bool _isInteracting;
        private bool _isConstructed;

        [Inject]
        public void Construct(
            IInputService inputService,
            IInventoryService inventoryService,
            IPopupService popupService,
            IPlayerInteractionContext context)
        {
            _inputService = inputService ?? throw new ArgumentNullException(nameof(inputService));
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            _popupService = popupService ?? throw new ArgumentNullException(nameof(popupService));
            _context = context ?? throw new ArgumentNullException(nameof(context));

            _isConstructed = true;
        }

        private void OnEnable()
        {
            if (_isConstructed)
                SubscribeToInput();
        }

        private void OnDisable()
        {
            _disposables.Clear();
            _currentInteractable = null;
            _isInteracting = false;
        }

        public void HandleTriggerEnter(Collider2D other)
        {
            TryEnterInteractable(other);
        }

        public void HandleTriggerExit(Collider2D other)
        {
            TryExitInteractable(other);
        }

        private void SubscribeToInput()
        {
            _disposables.Clear();

            if (_inputService?.Gameplay?.Interact == null)
                return;

            _inputService.Gameplay.Interact.Started
                .Where(pressed => pressed)
                .Subscribe(_ => TryInteractAsync().Forget())
                .AddTo(_disposables);
        }

        private async UniTask TryInteractAsync()
        {
            if (_isInteracting)
                return;

            if (_currentInteractable == null)
                return;

            if (!_currentInteractable.CanInteract)
                return;

            _isInteracting = true;

            try
            {
                await _currentInteractable.InteractAsync(
                    _context,
                    this.GetCancellationTokenOnDestroy());
            }
            finally
            {
                _isInteracting = false;
            }
        }

        private void TryEnterInteractable(Collider2D other)
        {
            IPlayerInteractable2D interactable =
                GetComponentFromCollider<IPlayerInteractable2D>(other);

            if (interactable == null)
                return;

            if (!interactable.CanInteract)
                return;

            Debug.Log($"Entered interactable: {other.name}");
            _currentInteractable = interactable;
        }

        private void TryExitInteractable(Collider2D other)
        {
            IPlayerInteractable2D interactable =
                GetComponentFromCollider<IPlayerInteractable2D>(other);

            if (interactable == null)
                return;

            if (ReferenceEquals(_currentInteractable, interactable))
            {
                Debug.Log($"Exited interactable: {other.name}");
                _currentInteractable = null;
            }
        }

        private static T GetComponentFromCollider<T>(Collider2D collider)
            where T : class
        {
            if (collider.TryGetComponent(out T component))
                return component;

            return collider.GetComponentInParent<T>();
        }
    }
}