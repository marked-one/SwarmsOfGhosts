using SwarmsOfGhosts.Gameplay.Enemy;
using SwarmsOfGhosts.UI.HUD.Player;
using Unity.Entities;
using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.UI.HUD.Infestation
{
    [CreateAssetMenu(fileName = "InfestationHUDInstaller", menuName = "Installers/InfestationHUDInstaller")]
    public class InfestationHUDInstaller : ScriptableObjectInstaller<PlayerHealthHUDInstaller>
    {
        [SerializeField] private InfestationView _infestationViewPrefab;

        public override void InstallBindings()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            var enemyHealthFacade = world.GetOrCreateSystem<EnemyHealthFacadeSystem>();

            Container
                .Bind<IInfestation>()
                .FromInstance(enemyHealthFacade)
                .AsSingle();

            Container
                .BindInterfacesTo<InfestationViewModel>()
                .AsSingle();

            Container
                .Bind<InfestationView>()
                .FromComponentInNewPrefab(_infestationViewPrefab)
                .UnderTransform(context => context.Container.Resolve<Canvas>().transform)
                .AsSingle()
                .NonLazy();
        }
    }
}