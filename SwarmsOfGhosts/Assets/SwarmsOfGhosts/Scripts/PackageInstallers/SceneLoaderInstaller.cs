using SceneManagement;
using SwarmsOfGhosts.App.Loading;
using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.PackageInstallers
{
    [CreateAssetMenu(fileName = "SceneLoaderInstaller", menuName = "Installers/SceneLoaderInstaller")]
    public class SceneLoaderInstaller : ScriptableObjectInstaller<SceneLoaderInstaller>
    {
        public override void InstallBindings() => SceneLoaderInstaller<SceneName>.Install(Container);
    }
}