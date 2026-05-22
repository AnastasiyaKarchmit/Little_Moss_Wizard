using Core.Audio.Configs;
using Core.Audio.Contracts;
using Features.Gameplay.CharacterController.Contracts;
using UnityEngine;
using VContainer;

namespace Features.Gameplay.CharacterController.Runtime
{
    [DisallowMultipleComponent]
    public sealed class PlayerAudioController2D : MonoBehaviour
    {
        private const int InvalidAudioId = 0;

        [Header("References")]
        [SerializeField] private Transform audioOrigin;

        [Header("Landing")]
        [SerializeField, Min(0f)] private float minLandingImpactToPlay = 4f;
        [SerializeField, Min(0.01f)] private float hardLandingImpact = 28f;
        [SerializeField, Range(0f, 1f)] private float minLandingVolume = 0.45f;
        [SerializeField, Range(0f, 1f)] private float maxLandingVolume = 1f;

        [Header("Footsteps")]
        [Tooltip("Use this only if you do not want to call footsteps from animation events.")]
        [SerializeField] private bool useAutomaticFootsteps;

        [SerializeField, Min(0f)] private float minFootstepSpeed = 1.5f;
        [SerializeField, Min(0.01f)] private float fullRunSpeed = 10f;

        [SerializeField, Min(0.01f)] private float slowFootstepInterval = 0.38f;
        [SerializeField, Min(0.01f)] private float fastFootstepInterval = 0.22f;

        private IAudioService _audioService;
        private IAudioDatabase _audioDatabase;
        private IPlayerMovementController _movementController;
        private IPlayerHealth _playerHealth;
        private IPlayerItemCollectionEvents _playerItemCollectionEvents;

        private float _nextFootstepTime;

        private bool _isSubscribed;

        private SoundConfig JumpSound => _audioDatabase?.Gameplay.PlayerJump;

        private SoundConfig LandSound => _audioDatabase?.Gameplay.PlayerLand;

        private SoundConfig DashSound => _audioDatabase?.Gameplay.PlayerDash;

        private SoundConfig FootstepSound => _audioDatabase?.Gameplay.PlayerFootstep;

        private SoundConfig HitSound => _audioDatabase?.Gameplay.PlayerHit;

        private SoundConfig ItemCollected => _audioDatabase?.Gameplay.PickupItem;

        [Inject]
        public void Construct(
            IAudioService audioService,
            IAudioDatabase audioDatabase,
            IPlayerMovementController movementController,
            IPlayerHealth playerHealth,
            IPlayerItemCollectionEvents playerItemCollectionEvents)
        {
            _audioService = audioService;
            _audioDatabase = audioDatabase;
            _movementController = movementController;
            _playerHealth = playerHealth;
            _playerItemCollectionEvents = playerItemCollectionEvents;
            
            SubscribeToEvents();
        }

        private void OnEnable()
        {
            if (_movementController == null 
                || _playerItemCollectionEvents == null
                || _playerHealth == null)
                return;
            
            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            if (_isSubscribed)
                return;
            
            _isSubscribed = true;
            
            _movementController.Jumped += OnJumped;
            _movementController.Dashed += OnDashed;
            _movementController.GroundedChanged += OnGroundedChanged;
            _playerItemCollectionEvents.ItemCollected += OnItemCollected;
            _playerHealth.Damaged += PlayHit;
        }

        private void OnDisable()
        {
            if (_movementController == null)
                return;

            UnsubscribeFromEvents();
        }

        private void UnsubscribeFromEvents()
        {
            _movementController.Jumped -= OnJumped;
            _movementController.Dashed -= OnDashed;
            _movementController.GroundedChanged -= OnGroundedChanged;
            _playerItemCollectionEvents.ItemCollected += OnItemCollected;
            _playerHealth.Damaged -= PlayHit;
            
            _isSubscribed = false;
        }

        private void Update()
        {
            if (!useAutomaticFootsteps)
                return;

            TryPlayAutomaticFootstep();
        }

        public void PlayFootstep()
        {
            if (_movementController == null)
                return;

            if (!_movementController.Grounded)
                return;

            if (_movementController.IsDashing)
                return;

            if (Mathf.Abs(_movementController.Velocity.x) < minFootstepSpeed)
                return;

            PlayAtPlayer(FootstepSound);
        }

        public void PlayHit(int damage)
        {
            PlayAtPlayer(HitSound);
        }

        private void OnJumped()
        {
            PlayAtPlayer(JumpSound);
        }

        private void OnDashed(Vector2 direction)
        {
            PlayAtPlayer(DashSound);
        }

        private void OnItemCollected()
        {
            PlayAtPlayer(ItemCollected);
        }
        private void OnGroundedChanged(bool grounded, float impactVelocity)
        {
            if (!grounded)
                return;

            if (impactVelocity < minLandingImpactToPlay)
                return;

            float t = Mathf.InverseLerp(
                minLandingImpactToPlay,
                hardLandingImpact,
                impactVelocity);

            float volume = Mathf.Lerp(minLandingVolume, maxLandingVolume, t);

            PlayAtPlayer(LandSound, volume);
        }

        private void TryPlayAutomaticFootstep()
        {
            if (_movementController == null)
                return;

            if (!_movementController.Grounded || _movementController.IsDashing)
                return;

            float speed = Mathf.Abs(_movementController.Velocity.x);

            if (speed < minFootstepSpeed)
                return;

            if (Time.time < _nextFootstepTime)
                return;

            PlayAtPlayer(FootstepSound);

            float speed01 = Mathf.Clamp01(speed / fullRunSpeed);

            float interval = Mathf.Lerp(
                slowFootstepInterval,
                fastFootstepInterval,
                speed01);

            _nextFootstepTime = Time.time + interval;
        }

        private int PlayAtPlayer(SoundConfig sound)
        {
            return PlayAtPlayer(sound, 1f);
        }

        private int PlayAtPlayer(SoundConfig sound, float volume)
        {
            if (_audioService == null || sound == null)
                return InvalidAudioId;

            Vector3 position = audioOrigin != null
                ? audioOrigin.position
                : transform.position;

            int id = _audioService.Play(sound, position);

            if (id != InvalidAudioId && volume < 0.99f)
                _audioService.SetVolume(id, volume);

            return id;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            hardLandingImpact = Mathf.Max(
                hardLandingImpact,
                minLandingImpactToPlay + 0.01f);

            fullRunSpeed = Mathf.Max(0.01f, fullRunSpeed);
        }
#endif
    }
}