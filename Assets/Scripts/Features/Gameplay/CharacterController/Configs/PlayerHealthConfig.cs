using UnityEngine;

namespace Features.Gameplay.CharacterController.Configs
{
    [CreateAssetMenu(
        fileName = "PlayerHealthConfig",
        menuName = "Configs/Player/Health Config")]
    public sealed class PlayerHealthConfig : ScriptableObject
    {
        [Header("Health")]
        [Min(1)] public int MaxHealth = 3;
        public bool StartWithFullHealth = true;

        [Header("Damage")]
        [Min(0f)] public float InvulnerabilityTimeAfterHit = 1f;

        [Header("Death")]
        public bool DisableGameplayOnDeath = true;
    }
}