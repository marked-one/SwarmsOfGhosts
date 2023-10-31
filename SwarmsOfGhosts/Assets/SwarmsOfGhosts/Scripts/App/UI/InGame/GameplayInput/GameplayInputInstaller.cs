using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.App.UI.InGame.GameplayInput
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