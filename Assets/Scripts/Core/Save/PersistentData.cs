using System;
using UnityEngine;

namespace Core.Save
{
    [Serializable]
    public sealed class PersistentData
    {
        public int Version = 1;

        public PlayerData Player = new();
        public GameplayData Gameplay = new();
        public SettingsData Settings = new();
    }
    [Serializable]
    public sealed class PlayerData
    {
        public int SoftCurrency;
        public int SelectedCharacterId;
    }
    
    [Serializable]
    public sealed class GameplayData
    {
        public int LastCompletedLevel;
        public CheckpointData Checkpoint = new();
    }
    
    [Serializable]
    public sealed class CheckpointData
    {
        public bool HasCheckpoint;
        public string SceneId;
        public string CheckpointId;

        public float PositionX;
        public float PositionY;
        public float PositionZ;

        public Vector3 GetPosition()
        {
            return new Vector3(PositionX, PositionY, PositionZ);
        }

        public void SetPosition(Vector3 position)
        {
            PositionX = position.x;
            PositionY = position.y;
            PositionZ = position.z;
        }

        public void Clear()
        {
            HasCheckpoint = false;
            SceneId = string.Empty;
            CheckpointId = string.Empty;

            PositionX = 0f;
            PositionY = 0f;
            PositionZ = 0f;
        }
    }
    
    [Serializable]
    public sealed class SettingsData
    {
        public float MasterVolume = 0.5f;
        public float MusicVolume = 1f;
        public float SfxVolume = 1f;
        public bool MasterMuted;
    }
}