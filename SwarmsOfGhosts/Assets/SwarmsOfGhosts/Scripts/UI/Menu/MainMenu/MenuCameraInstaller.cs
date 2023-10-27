using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.UI.Menu.MainMenu
{
    [CreateAssetMenu(fileName = "MenuCameraInstaller", menuName = "Installers/MenuCameraInstaller")]
    public class MenuCameraInstaller : ScriptableObjectInstaller<MenuCameraInstaller>
    {
        [SerializeField] private GameObject _cameraPrefab;

        public override void InstallBindings()
        {
            Container
                .Bind<Camera>()
                .FromComponentInNewPrefab(_cameraPrefab)
                .AsSingle()
                .NonLazy();
        }
    }
}