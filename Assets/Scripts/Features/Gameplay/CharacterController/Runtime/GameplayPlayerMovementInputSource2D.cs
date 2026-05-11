using System;
using Core.Input.Contracts;
using Features.Gameplay.CharacterController.Contracts;
using Features.Gameplay.CharacterController.Data;
using R3;

namespace Features.Gameplay.CharacterController.Runtime
{
    public sealed class GameplayPlayerMovementInputSource2D : IPlayerMovementInputSource, IDisposable
    {
        private readonly IInputService _inputService;
        private readonly CompositeDisposable _disposables = new();

        private bool _enabled = true;
        private bool _isBound;

        private bool _jumpPressed;
        private bool _dashPressed;

        private readonly IGameplayInput _gameplayInput;
        
        public GameplayPlayerMovementInputSource2D(IInputService inputService)
        {
            _inputService = inputService;
            _gameplayInput = _inputService.Gameplay;
        }

        public PlayerMovementInputFrame ConsumeFrameInput()
        {
            if (!_enabled)
            {
                ClearOneFrameActions();
                return default;
            }

            if (!TryBind())
                return default;

            PlayerMovementInputFrame frame = new PlayerMovementInputFrame(
                move: _gameplayInput.Move.Value,
                jumpPressed: _jumpPressed,
                jumpHeld: _gameplayInput.Jump.Value,
                dashPressed: _dashPressed);

            ClearOneFrameActions();

            return frame;
        }

        public void SetInputEnabled(bool enabled)
        {
            _enabled = enabled;

            if (!enabled)
                ClearOneFrameActions();
        }

        private bool TryBind()
        {
            if (_isBound)
                return true;

            if (_inputService.Gameplay == null)
                return false;

            _gameplayInput.Jump.Started
                .Where(isPressed => isPressed)
                .Subscribe(_ => _jumpPressed = true)
                .AddTo(_disposables);

            _gameplayInput.Dash.Started
                .Where(isPressed => isPressed)
                .Subscribe(_ => _dashPressed = true)
                .AddTo(_disposables);

            _isBound = true;
            return true;
        }

        private void ClearOneFrameActions()
        {
            _jumpPressed = false;
            _dashPressed = false;
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}