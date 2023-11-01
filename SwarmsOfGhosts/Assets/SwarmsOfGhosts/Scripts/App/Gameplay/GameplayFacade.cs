using System;
using Cysharp.Threading.Tasks;
using SwarmsOfGhosts.App.Gameplay.Enemy;
using SwarmsOfGhosts.App.Gameplay.Pause;
using SwarmsOfGhosts.App.Gameplay.Player;
using SwarmsOfGhosts.App.Gameplay.Restart;
using UniRx;
using Unity.Entities;
using Unity.Scenes;
using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.App.Gameplay
{
    public interface ISubSceneLoader
    {
        public UniTask LoadSubScene();
    }

    public interface IRestartable
    {
        public UniTask Restart();
    }

    public interface IPausable
    {
        public bool IsPaused { set; }
    }

    public interface IMovable
    {
        public Vector2 Movement { set; }
    }

    public interface IPlayerPosition
    {
        public IObservable<Vector3> PlayerPosition { get; }
    }

    public interface IPlayerHealth
    {
        public IReadOnlyReactiveProperty<float> CurrentPlayerHealth { get; }
        public IReadOnlyReactiveProperty<float> MaxPlayerHealth { get; }
    }

    public interface IInfestation
    {
        public IReadOnlyReactiveProperty<float> CurrentInfestation { get; }
        public IReadOnlyReactiveProperty<float> MaxInfestation { get; }
    }

    public interface IEnemySpawn
    {
        public int EnemyGridSize { get; set; }
    }

    public class GameplayFacade : IInitializable, IDisposable,
        ISubSceneLoader, IRestartable, IPausable, IMovable, IPlayerPosition, IPlayerHealth, IInfestation, IEnemySpawn
    {
        private readonly RestartSystem _restartSystem;
        private readonly PlayerInputSystem _playerInputSystem;
        private readonly EnemySpawnSystem _enemySpawnSystem;
        private readonly PauseSystem _pauseSystem;
        private readonly SceneSystem _sceneSystem;

        public IReadOnlyReactiveProperty<float> CurrentPlayerHealth { get; }
        public IReadOnlyReactiveProperty<float> MaxPlayerHealth { get; }

        public IReadOnlyReactiveProperty<float> CurrentInfestation { get; }
        public IReadOnlyReactiveProperty<float> MaxInfestation { get; }

        public IObservable<Vector3> PlayerPosition { get; }

        public int EnemyGridSize
        {
            get => _enemySpawnSystem.GridSize;
            set => _enemySpawnSystem.GridSize = value;
        }

        public Vector2 Movement
        {
            set => _playerInputSystem.Movement = value;
        }

        public bool IsPaused
        {
            set => _pauseSystem.IsPaused = value;
        }

        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();

        private readonly SubScene _subScene;

        [Inject]
        private GameplayFacade(SubScene subScene)
        {
            _subScene = subScene;

            var world = World.DefaultGameObjectInjectionWorld;

            _sceneSystem = world.GetOrCreateSystem<SceneSystem>();

            _restartSystem = world.GetOrCreateSystem<RestartSystem>();
            _pauseSystem = world.GetOrCreateSystem<PauseSystem>();

            _playerInputSystem = world.GetOrCreateSystem<PlayerInputSystem>();
            var playerHealthFacadeSystem = world.GetOrCreateSystem<PlayerHealthSystem>();
            CurrentPlayerHealth = playerHealthFacadeSystem.Current;
            MaxPlayerHealth = playerHealthFacadeSystem.Max;

            var playerMovementSystem = world.GetOrCreateSystem<PlayerMovementSystem>();
            PlayerPosition = playerMovementSystem.Position;

            var enemyHealthFacadeSystem = world.GetOrCreateSystem<EnemyHealthSystem>();
            _enemySpawnSystem = world.GetOrCreateSystem<EnemySpawnSystem>();
            CurrentInfestation = enemyHealthFacadeSystem.Current;
            MaxInfestation = enemyHealthFacadeSystem.Max;
        }

        public void Initialize() => _pauseSystem.IsPaused = false;

        public async UniTask LoadSubScene()
        {
            var handle = _sceneSystem.LoadSceneAsync(_subScene.SceneGUID);
            await UniTask.WaitUntil(() => _sceneSystem.IsSceneLoaded(handle));
        }

        public UniTask Restart() => _restartSystem.Restart();

        public void Dispose()
        {
            _restartSystem.DestroyIsPlayingEntity();
            _subscriptions.Dispose();
        }
    }
}