using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.App.Gameplay.Input
{
    [CreateAssetMenu(fileName = "GameplayInputInstaller", menuName = "Installers/GameplayInputInstaller")]
    public class GameplayInputInstaller : ScriptableObjectInstaller<GameplayInputInstaller>
    {
        public override void InstallBindings() =>
            Container
                .BindInterfacesTo<GameplayInputViewModel>()
                .AsSingle();
    }
}