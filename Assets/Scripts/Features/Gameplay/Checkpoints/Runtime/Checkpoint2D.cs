using Features.Gameplay.CharacterController.Runtime;
using UnityEngine;
using VContainer;

namespace Features.Gameplay.Checkpoints.Runtime
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public sealed class Checkpoint2D : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] private string id;

        [Header("Spawn")]
        [SerializeField] private Transform spawnPoint;

        [Header("Activation")]
        [SerializeField] private bool activateOnlyOnce = false;

        private CheckpointService _checkpointService;
        private bool _activated;

        public string Id => string.IsNullOrWhiteSpace(id) ? gameObject.name : id;

        public Vector3 SpawnPosition =>
            spawnPoint != null ? spawnPoint.position : transform.position;
        
        public void RegisterCheckpoint(CheckpointService checkpointService)
        {
            _checkpointService = checkpointService;

            if (isActiveAndEnabled)
                _checkpointService.Register(this);
        }

        private void Awake()
        {
            Collider2D trigger = GetComponent<Collider2D>();
            trigger.isTrigger = true;
        }

        private void OnEnable()
        {
            _checkpointService?.Register(this);
        }

        private void OnDisable()
        {
            _checkpointService?.Unregister(this);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (activateOnlyOnce && _activated)
                return;

            if (!other.GetComponentInParent<PlayerController>())
                return;

            _activated = true;
            _checkpointService?.Activate(this);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(id))
                id = gameObject.name;

            Collider2D trigger = GetComponent<Collider2D>();

            if (trigger != null)
                trigger.isTrigger = true;
        }
#endif
    }
}