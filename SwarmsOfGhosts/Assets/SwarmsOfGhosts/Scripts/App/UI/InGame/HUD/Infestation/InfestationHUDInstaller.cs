using SwarmsOfGhosts.App.UI.InGame.HUD.Player;
using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.App.UI.InGame.HUD.Infestation
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
                .NonLazy();
        }
    }
}