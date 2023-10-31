using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.App.UI.Menu
{
    [CreateAssetMenu(fileName = "MenuLightInstaller", menuName = "Installers/MenuLightInstaller")]
    public class MenuLightInstaller : ScriptableObjectInstaller<MenuLightInstaller>
    {
        [SerializeField] private Light _lightPrefab;

        public override void InstallBindings() =>
            Container
                .Bind<Light>()
                .FromComponentInNewPrefab(_lightPrefab)
                .AsSingle()
                .NonLazy();
    }
}