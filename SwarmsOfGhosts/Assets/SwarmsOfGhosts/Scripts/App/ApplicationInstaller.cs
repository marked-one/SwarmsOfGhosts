using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.App
{
    [CreateAssetMenu(fileName = "ApplicationInstaller", menuName = "Installers/ApplicationInstaller")]
    public class ApplicationInstaller : ScriptableObjectInstaller<ApplicationInstaller>
    {
        public override void InstallBindings()
        {
            Container
                .BindInterfacesTo<GameApplication>()
                .AsSingle();
        }
    }
}