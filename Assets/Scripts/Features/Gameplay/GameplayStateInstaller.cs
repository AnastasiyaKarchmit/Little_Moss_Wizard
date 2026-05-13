using Core.AppStates.Components;
using Features.Gameplay.CharacterController.Configs;
using Features.Gameplay.CharacterController.Runtime;
using Features.Gameplay.Infrastructure;
using Features.Gameplay.Infrastructure.States.GameplayState;
using Features.Gameplay.Infrastructure.States.PauseState;
using Features.Shared.SettingsState;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Features.Gameplay
{
    public class GameplayStateInstaller : AppStateInstaller
    {
        [SerializeField] private GameObject player;
        [SerializeField] private PlayerMovementConfig playerMovementConfig;
        public override void RegisterDependencies(IContainerBuilder builder)
        {
            builder.Register<GameplayModel>(Lifetime.Singleton);
            builder.Register<GameplayPresenter>(Lifetime.Singleton);
            builder.Register<PausePresenter>(Lifetime.Singleton);
            builder.Register<SettingsPresenter>(Lifetime.Singleton);
            builder.Register<GameplayFlowController>(Lifetime.Singleton);
            builder.Register<GameplayAudioController>(Lifetime.Singleton);
            builder.Register<GameplayAppStateController>(Lifetime.Singleton);
            RegisterPlayer(builder);
        }

        private void RegisterPlayer(IContainerBuilder builder)
        {
            builder.RegisterInstance(playerMovementConfig);
            
            builder.Register<GameplayPlayerMovementInputSource2D>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.RegisterComponent(player.GetComponent<PlayerMovementController2D>())
                .AsImplementedInterfaces()
                .AsSelf();
            
            builder.RegisterComponent(player.GetComponent<PlayerController>())
                .AsImplementedInterfaces()
                .AsSelf();
            
            builder.RegisterComponent(player.GetComponent<PlayerAudioController2D>())
                .AsSelf();
        }
    }
}