using SwarmsOfGhosts.App.Gameplay.HUD.Player;
using SwarmsOfGhosts.App.Utilities;
using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.App.Gameplay.HUD.Infestation
{
    [CreateAssetMenu(fileName = "InfestationHUDInstaller", menuName = "Installers/InfestationHUDInstaller")]
    public class InfestationHUDInstaller : ScriptableObjectInstaller<PlayerHealthHUDInstaller>
    {
        [SerializeField] private InfestationView _infestationViewPrefab;

        public override void InstallBindings()
        {
            Container
                .BindInterfacesTo<InfestationViewModel>()
                .AsSingle();

            Container
                .Bind<InfestationView>()
                .FromComponentInNewPrefab(_infestationViewPrefab)
                .UnderTransform(context => context.Container.Resolve<Canvas>().transform)
                .AsSingle()
                .OnInstantiated<InfestationView>((context, view) =>
                {
                    var organizer = context.Container.Resolve<IHierarchyOrganizer>();
                    var parent = context.Container.Resolve<Canvas>();
                    organizer.AddChild(parent.transform, view.transform, 0);
                })
                .NonLazy();
        }
    }
}