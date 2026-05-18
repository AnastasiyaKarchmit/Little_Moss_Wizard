using Core.AppStates.Components;
using Features.Gameplay.CharacterController.Configs;
using Features.Gameplay.CharacterController.Contracts;
using Features.Gameplay.CharacterController.Runtime;
using Features.Gameplay.Infrastructure;
using Features.Gameplay.Infrastructure.States.GameplayState;
using Features.Gameplay.Infrastructure.States.InventoryState;
using Features.Gameplay.Infrastructure.States.PauseState;
using Features.Gameplay.Inventory;
using Features.Gameplay.Inventory.Contracts;
using Features.Gameplay.Inventory.Runtime;
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
            RegisterInventory(builder);
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
            
            builder.RegisterComponent(player.GetComponentInChildren<PlayerAudioController2D>())
                .AsSelf();
            
            builder.RegisterComponent(player.GetComponentInChildren<PlayerHealth>())
                .AsImplementedInterfaces()
                .AsSelf();
            
            builder.RegisterComponent(player.GetComponentInChildren<PlayerBoostController>())
                .AsSelf()
                .As<IPlayerBoostController>();

            builder.RegisterComponent(player.GetComponent<PlayerCollisionController2D>());

            builder.RegisterComponent(player.GetComponent<PlayerMovementAnimator2D>());
        }

        private void RegisterInventory(IContainerBuilder builder)
        {
            builder.Register<InventoryItemUseContext>(Lifetime.Singleton)
                .As<IInventoryItemUseContext>();
            
            builder.Register<InventoryService>(Lifetime.Singleton)
                .As<IInventoryService>();
            
            builder.Register<InventoryPresenter>(Lifetime.Singleton)
                .AsSelf();
        }
    }
}