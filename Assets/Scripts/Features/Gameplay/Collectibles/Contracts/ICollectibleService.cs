using System;
using Core.Save;
using Features.Gameplay.Collectibles.Runtime;

namespace Features.Gameplay.Collectibles.Contracts
{
    public interface ICollectibleService : ISaveDataProvider
    {
        bool IsCollected(string id);
        bool MarkCollected(string id);
    }
}