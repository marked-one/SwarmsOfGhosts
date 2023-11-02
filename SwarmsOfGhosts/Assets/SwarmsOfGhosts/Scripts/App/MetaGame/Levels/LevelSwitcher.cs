using System;
using Cysharp.Threading.Tasks;
using SwarmsOfGhosts.App.Gameplay;
using SwarmsOfGhosts.App.MetaGame.Saves;
using UniRx;
using Zenject;

namespace SwarmsOfGhosts.App.MetaGame.Levels
{
    public enum LevelState
    {
        Playing,
        LevelCompleted,
        GameOver,
        GameCompleted
    }

    public interface ILevelSwitcher
    {
        public IReadOnlyReactiveProperty<int> LevelScore { get; }
        public IReadOnlyReactiveProperty<LevelState> LevelState { get; }
        public UniTask StartNextLevel();
    }

    public class LevelSwitcher : ILevelSwitcher, IInitializable, IDisposable
    {
        private readonly ISave _scoreSave;
        private readonly ILevelsConfig _levelsConfig;
        private readonly IRestartable _restartableLevel;
        private readonly IEnemies _enemies;
        private readonly IPlayer _player;
        private readonly IEnemySpawn _enemySpawn;

        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();

        private int _currentLevel;

        private bool _areEnemiesAlive;
        private bool _isPlayerAlive;

        private readonly ReactiveProperty<bool> _isLevelPlaying = new ReactiveProperty<bool>();

        private readonly ReactiveProperty<int> _levelScore = new ReactiveProperty<int>(0);
        public IReadOnlyReactiveProperty<int> LevelScore => _levelScore;

        private readonly ReactiveProperty<LevelState> _levelState = new ReactiveProperty<LevelState>();
        public IReadOnlyReactiveProperty<LevelState> LevelState => _levelState;

        [Inject]
        private LevelSwitcher(ISave scoreSave, ILevelsConfig levelsConfig, IRestartable restartableLevel,
            IPlayer player, IEnemies enemies, IEnemySpawn enemySpawn)
        {
            _scoreSave = scoreSave;
            _levelsConfig = levelsConfig;
            _restartableLevel = restartableLevel;
            _player = player;
            _enemies = enemies;
            _enemySpawn = enemySpawn;
        }

        public void Initialize()
        {
            _enemySpawn.EnemyGridSize = _levelsConfig.EnemyGridStep;
            _currentLevel = 1;
            _levelState.Value = Levels.LevelState.Playing;

            _isLevelPlaying
                .Skip(1)
                .Subscribe(value =>
                {
                    if (value)
                        StartLevel();
                    else
                        EndLevel();
                })
                .AddTo(_subscriptions);

            _enemies.AreEnemiesAlive
                .Subscribe(value =>
                {
                    _areEnemiesAlive = value;
                    _isLevelPlaying.Value = _areEnemiesAlive && _isPlayerAlive;
                })
                .AddTo(_subscriptions);

            _player.IsPlayerAlive
                .Subscribe(value =>
                {
                    _isPlayerAlive = value;
                    _isLevelPlaying.Value = _areEnemiesAlive && _isPlayerAlive;
                })
                .AddTo(_subscriptions);

            void StartLevel() => _levelState.Value = Levels.LevelState.Playing;

            void EndLevel()
            {
                if (!_areEnemiesAlive)
                {
                    UpdateLevelScore(_currentLevel);

                    _levelState.Value = _currentLevel == _levelsConfig.EnemyGridMaxSteps
                        ? Levels.LevelState.GameCompleted
                        : Levels.LevelState.LevelCompleted;
                }
                else if (!_isPlayerAlive)
                {
                    UpdateLevelScore(_currentLevel - 1);
                    _levelState.Value = Levels.LevelState.GameOver;
                }

                void UpdateLevelScore(int value)
                {
                    var savedScore = _scoreSave.Score.Value;
                    if (value > savedScore)
                        _scoreSave.SaveScore(value);

                    _levelScore.Value = value;
                }
            }
        }

        public async UniTask StartNextLevel()
        {
            _enemySpawn.EnemyGridSize += _levelsConfig.EnemyGridStep;
            await _restartableLevel.Restart();
            _currentLevel++;
        }

        public void Dispose() => _subscriptions.Dispose();
    }
}