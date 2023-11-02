using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.App.Gameplay.MouseCursor
{
    [CreateAssetMenu(fileName = "GameplayCursorInstaller", menuName = "Installers/GameplayCursorInstaller")]
    public class GameplayCursorInstaller : ScriptableObjectInstaller<GameplayCursorInstaller>
    {
        public override void InstallBindings()
        {
            Container
                .BindInterfacesTo<GameplayCursorViewModel>()
                .AsSingle();

            Container
                .BindInterfacesTo<GameplayCursorView>()
                .AsSingle();
        }
    }
}