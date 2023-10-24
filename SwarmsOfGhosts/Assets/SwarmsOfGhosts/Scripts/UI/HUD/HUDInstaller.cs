using SwarmsOfGhosts.Gameplay.Utilities;
using Unity.Entities;
using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.UI
{
    [CreateAssetMenu(fileName = "HUDInstaller", menuName = "Installers/HUDInstaller")]
    public class HUDInstaller : ScriptableObjectInstaller<HUDInstaller>
    {
        [SerializeField] private PlayerHealthView _playerHealthViewPrefab;

        public override void InstallBindings()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            var playerHealthListener = world.GetOrCreateSystem<PlayerHealthFacade>();

            Container
                .Bind<IPlayerHealth>()
                .FromInstance(playerHealthListener)
                .AsSingle();

            Container
                .BindInterfacesTo<PlayerHealthViewModel>()
                .AsSingle();

            Container
                .Bind<PlayerHealthView>()
                .FromComponentInNewPrefab(_playerHealthViewPrefab)
                .UnderTransform(context => context.Container.Resolve<Canvas>().transform)
                .AsSingle()
                .NonLazy();
        }
    }
}