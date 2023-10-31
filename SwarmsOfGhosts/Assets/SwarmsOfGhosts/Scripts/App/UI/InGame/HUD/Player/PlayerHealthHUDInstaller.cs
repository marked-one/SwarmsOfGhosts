using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.App.UI.InGame.HUD.Player
{
    [CreateAssetMenu(fileName = "PlayerHealthHUDInstaller", menuName = "Installers/PlayerHealthHUDInstaller")]
    public class PlayerHealthHUDInstaller : ScriptableObjectInstaller<PlayerHealthHUDInstaller>
    {
        [SerializeField] private PlayerHealthView _playerHealthViewPrefab;

        public override void InstallBindings()
        {
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