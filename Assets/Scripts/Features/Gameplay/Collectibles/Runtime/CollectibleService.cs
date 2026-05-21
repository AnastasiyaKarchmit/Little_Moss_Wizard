using System;
using System.Collections.Generic;
using Core.Save;
using Cysharp.Threading.Tasks;
using Features.Gameplay.Collectibles.Contracts;
using UnityEngine;

namespace Features.Gameplay.Collectibles.Runtime
{
    public sealed class CollectibleService : ICollectibleService, IDisposable
    {
        private readonly ISaveSystem _saveSystem;
        private readonly HashSet<string> _collectedIds = new();

        private bool _isDisposed;

        public CollectibleService(ISaveSystem saveSystem)
        {
            _saveSystem = saveSystem ?? throw new ArgumentNullException(nameof(saveSystem));
            _saveSystem.Register(this);
        }

        public UniTask LoadAsync(PersistentData data)
        {
            _collectedIds.Clear();

            if (data?.Gameplay?.Collectibles?.CollectedIds == null)
                return UniTask.CompletedTask;

            foreach (string id in data.Gameplay.Collectibles.CollectedIds)
            {
                if (!string.IsNullOrWhiteSpace(id))
                    _collectedIds.Add(id);
            }

            return UniTask.CompletedTask;
        }

        public void Save(PersistentData data)
        {
            if (data == null)
                return;

            data.Gameplay.Collectibles ??= new CollectiblesData();

            data.Gameplay.Collectibles.CollectedIds.Clear();
            data.Gameplay.Collectibles.CollectedIds.AddRange(_collectedIds);
        }

        public bool IsCollected(string id)
        {
            return !string.IsNullOrWhiteSpace(id) &&
                   _collectedIds.Contains(id);
        }

        public bool MarkCollected(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return false;

            return _collectedIds.Add(id);
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            _saveSystem.Unregister(this);
        }
    }
}