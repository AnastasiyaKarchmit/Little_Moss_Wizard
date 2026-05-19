using System;
using System.Collections.Generic;
using Core.Save;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

namespace Features.Gameplay.Checkpoints.Runtime
{
    public sealed class CheckpointService : ISaveDataProvider
    {
        private readonly Dictionary<string, Checkpoint2D> _registeredCheckpoints = new();
        private readonly ISaveSystem _saveSystem;

        private bool _hasCheckpoint;
        private string _sceneId;
        private string _checkpointId;
        private Vector3 _savedPosition;

        public event Action<Checkpoint2D> CheckpointActivated;

        public bool HasCheckpoint => _hasCheckpoint;
        public string CurrentCheckpointId => _checkpointId;
        public string CurrentSceneId => _sceneId;

        public CheckpointService(ISaveSystem saveSystem)
        {
            _saveSystem = saveSystem;
            _saveSystem.Register(this);
        }
        
        public UniTask LoadAsync(PersistentData data)
        {
            if (data == null)
                return UniTask.CompletedTask;

            CheckpointData checkpointData = data.Gameplay.Checkpoint;

            if (checkpointData == null || !checkpointData.HasCheckpoint)
            {
                ClearRuntimeData();
                return UniTask.CompletedTask;
            }

            _hasCheckpoint = true;
            _sceneId = checkpointData.SceneId;
            _checkpointId = checkpointData.CheckpointId;
            _savedPosition = checkpointData.GetPosition();

            return UniTask.CompletedTask;
        }

        public void Save(PersistentData data)
        {
            if (data == null)
                return;

            data.Gameplay.Checkpoint ??= new CheckpointData();

            CheckpointData checkpointData = data.Gameplay.Checkpoint;

            checkpointData.HasCheckpoint = _hasCheckpoint;
            checkpointData.SceneId = _sceneId;
            checkpointData.CheckpointId = _checkpointId;
            checkpointData.SetPosition(_savedPosition);
        }

        public void Register(Checkpoint2D checkpoint)
        {
            if (checkpoint == null)
                return;

            string id = checkpoint.Id;

            if (string.IsNullOrWhiteSpace(id))
            {
                Debug.LogError("Checkpoint id is empty.", checkpoint);
                return;
            }

            _registeredCheckpoints[id] = checkpoint;
        }

        public void Unregister(Checkpoint2D checkpoint)
        {
            if (checkpoint == null)
                return;

            string id = checkpoint.Id;

            if (_registeredCheckpoints.TryGetValue(id, out Checkpoint2D registered) &&
                registered == checkpoint)
            {
                _registeredCheckpoints.Remove(id);
            }
        }

        public void Activate(Checkpoint2D checkpoint)
        {
            if (checkpoint == null)
                return;

            _hasCheckpoint = true;
            _sceneId = SceneManager.GetActiveScene().name;
            _checkpointId = checkpoint.Id;
            _savedPosition = checkpoint.SpawnPosition;

            CheckpointActivated?.Invoke(checkpoint);
        }

        public bool TryGetSpawnPosition(out Vector3 position)
        {
            position = default;

            if (!_hasCheckpoint)
                return false;

            string currentSceneId = SceneManager.GetActiveScene().name;

            if (!string.Equals(_sceneId, currentSceneId, StringComparison.Ordinal))
                return false;

            if (!string.IsNullOrWhiteSpace(_checkpointId) &&
                _registeredCheckpoints.TryGetValue(_checkpointId, out Checkpoint2D checkpoint) &&
                checkpoint != null)
            {
                position = checkpoint.SpawnPosition;
                return true;
            }

            position = _savedPosition;
            return true;
        }

        public bool IsCurrentCheckpoint(Checkpoint2D checkpoint)
        {
            if (checkpoint == null)
                return false;

            return _hasCheckpoint &&
                   string.Equals(_checkpointId, checkpoint.Id, StringComparison.Ordinal);
        }

        public void ClearCheckpoint()
        {
            ClearRuntimeData();
        }

        private void ClearRuntimeData()
        {
            _hasCheckpoint = false;
            _sceneId = string.Empty;
            _checkpointId = string.Empty;
            _savedPosition = default;
        }
    }
}