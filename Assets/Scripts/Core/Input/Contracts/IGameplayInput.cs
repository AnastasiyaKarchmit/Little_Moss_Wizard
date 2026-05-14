using UnityEngine;

namespace Core.Input.Contracts
{
    public interface IGameplayInput
    {
        IInputAction<Vector2> Move { get; }
        IInputAction<Vector2> Look { get; }
        IInputAction<bool> Attack { get; }
        IInputAction<bool> Interact { get; }
        IInputAction<bool> Dash { get; }
        IInputAction<bool> Jump { get; }
        IInputAction<bool> Crouch { get; }
        
        IInputAction<bool> Inventory { get; }

        void SetActive(bool active);
    }
}