using System;

namespace Features.Gameplay.CharacterController.Contracts
{
    public interface IPlayerItemCollectionEvents
    {
        event Action ItemCollected;
    }
}