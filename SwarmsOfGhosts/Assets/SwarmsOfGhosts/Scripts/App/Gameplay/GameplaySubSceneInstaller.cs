using Unity.Scenes;
using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.App.Gameplay
{
    public class GameplaySubSceneInstaller : MonoInstaller
    {
        [SerializeField] private SubScene _subScene;

        public override void InstallBindings() =>
            Container
                .Bind<SubScene>()
                .FromInstance(_subScene)
                .AsSingle();
    }
}