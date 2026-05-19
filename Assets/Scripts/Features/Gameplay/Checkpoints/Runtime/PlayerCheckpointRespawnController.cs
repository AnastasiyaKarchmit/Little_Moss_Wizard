using System;
using Cysharp.Threading.Tasks;
using Features.Gameplay.CharacterController.Runtime;
using UnityEngine;
using VContainer;

namespace Features.Gameplay.Checkpoints.Runtime
{
    [DisallowMultipleComponent]
    public sealed class PlayerCheckpointRespawnController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private PlayerHealth health;
        [SerializeField] private PlayerMovementController2D movement;
        [SerializeField] private Rigidbody2D rigidbody2D;
        [SerializeField] private Transform playerRoot;

        [Header("Spawn")]
        [SerializeField] private Transform defaultSpawnPoint;
        [SerializeField] private bool spawnOnStart = true;

        [Header("Respawn")]
        [SerializeField, Min(0f)] private float respawnDelay = 0.4f;
        [SerializeField] private bool resetHealthOnRespawn = true;
        [SerializeField] private bool disableGameplayDuringRespawn = true;

        private CheckpointService _checkpointService;
        private bool _isRespawning;

        [Inject]
        public void Construct(CheckpointService checkpointService)
        {
            _checkpointService = checkpointService;
            var checkpoints = GetComponentsInChildren<Checkpoint2D>(true);
            if (checkpoints.Length > 0)
            {
                foreach (var checkpoint in checkpoints)
                {
                    checkpoint.RegisterCheckpoint(checkpointService);
                }
            }
        }

        private void Awake()
        {
            ResolveReferences();
        }

        private async void Start()
        {
            if (!spawnOnStart)
                return;

            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

            SpawnAtSavedPoint();
        }

        private void OnEnable()
        {
            if (health != null)
                health.Died += OnDied;
        }

        private void OnDisable()
        {
            if (health != null)
                health.Died -= OnDied;
        }

        public void SpawnAtSavedPoint()
        {
            MovePlayerTo(GetSpawnPosition());
            ClearPhysicsVelocity();
        }

        public void Respawn()
        {
            RespawnAsync().Forget();
        }

        private void OnDied()
        {
            if (_isRespawning)
                return;

            RespawnAsync().Forget();
        }

        private async UniTask RespawnAsync()
        {
            _isRespawning = true;

            if (disableGameplayDuringRespawn)
                SetGameplayActive(false);

            ClearPhysicsVelocity();

            await UniTask.Delay(
                TimeSpan.FromSeconds(respawnDelay),
                DelayType.DeltaTime,
                PlayerLoopTiming.Update,
                this.GetCancellationTokenOnDestroy());

            MovePlayerTo(GetSpawnPosition());
            ClearPhysicsVelocity();

            if (resetHealthOnRespawn && health != null)
                health.ResetHealth();

            if (disableGameplayDuringRespawn)
                SetGameplayActive(true);

            _isRespawning = false;
        }

        private Vector3 GetSpawnPosition()
        {
            if (_checkpointService != null &&
                _checkpointService.TryGetSpawnPosition(out Vector3 checkpointPosition))
            {
                return checkpointPosition;
            }

            if (defaultSpawnPoint != null)
                return defaultSpawnPoint.position;

            return playerRoot != null
                ? playerRoot.position
                : transform.position;
        }

        private void MovePlayerTo(Vector3 position)
        {
            if (playerRoot == null)
                playerRoot = transform;

            playerRoot.position = position;
        }

        private void SetGameplayActive(bool active)
        {
            if (playerController != null)
            {
                playerController.SetGameplayActive(active);
                return;
            }

            if (movement != null)
                movement.SetActive(active);
        }

        private void ClearPhysicsVelocity()
        {
            if (movement != null)
                movement.ClearExternalVelocity();

            if (rigidbody2D == null)
                return;

#if UNITY_6000_0_OR_NEWER
            rigidbody2D.linearVelocity = Vector2.zero;
#else
            rigidbody2D.velocity = Vector2.zero;
#endif

            rigidbody2D.angularVelocity = 0f;
        }

        private void ResolveReferences()
        {
            if (playerRoot == null)
                playerRoot = transform;

            if (playerController == null)
                playerController = GetComponentInParent<PlayerController>();

            if (health == null)
                health = GetComponentInParent<PlayerHealth>();

            if (movement == null)
                movement = GetComponentInParent<PlayerMovementController2D>();

            if (rigidbody2D == null)
                rigidbody2D = GetComponentInParent<Rigidbody2D>();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ResolveReferences();
        }
#endif
    }
}