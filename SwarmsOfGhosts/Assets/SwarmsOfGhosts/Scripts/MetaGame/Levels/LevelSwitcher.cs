using System;
using Cysharp.Threading.Tasks;
using SwarmsOfGhosts.Gameplay.Enemy;
using SwarmsOfGhosts.Gameplay.Player;
using SwarmsOfGhosts.Gameplay.Restart;
using SwarmsOfGhosts.MetaGame.Saves;
using UniRx;
using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.MetaGame.Levels
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
        private readonly IInfestation _enemyInfestation;
        private readonly IPlayerHealth _playerHealth;
        private readonly IEnemySpawn _enemySpawn;

        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();

        private int _currentLevel;

        private readonly ReactiveProperty<int> _levelScore = new ReactiveProperty<int>(0);
        public IReadOnlyReactiveProperty<int> LevelScore => _levelScore;

        private readonly ReactiveProperty<LevelState> _levelState = new ReactiveProperty<LevelState>();
        public IReadOnlyReactiveProperty<LevelState> LevelState => _levelState;

        [Inject]
        private LevelSwitcher(ISave scoreSave, ILevelsConfig levelsConfig, IRestartable restartableLevel,
            IInfestation enemyInfestation, IPlayerHealth playerHealth, IEnemySpawn enemySpawn)
        {
            _scoreSave = scoreSave;
            _levelsConfig = levelsConfig;
            _restartableLevel = restartableLevel;
            _enemyInfestation = enemyInfestation;
            _playerHealth = playerHealth;
            _enemySpawn = enemySpawn;
        }

        public void Initialize()
        {
            _enemySpawn.GridSize = _levelsConfig.GridStep;
            _currentLevel = 1;
            _levelState.Value = Levels.LevelState.Playing;

            var areEnemiesDead = false;
            var isPlayerDead = false;

            // Note: skip the initialization because values are 0s at first

            _enemyInfestation.Current
                .Skip(1)
                .Subscribe(value =>
                {
                    areEnemiesDead = Mathf.Approximately(value, 0f);
                    EndLevel();
                })
                .AddTo(_subscriptions);

            _playerHealth.Current
                .Skip(1)
                .Subscribe(value =>
                {
                    isPlayerDead = Mathf.Approximately(value, 0f);
                    EndLevel();
                })
                .AddTo(_subscriptions);

            void EndLevel()
            {
                if (areEnemiesDead && isPlayerDead)
                    return;

                if (areEnemiesDead)
                {
                    UpdateLevelScore(_currentLevel);

                    _levelState.Value = _currentLevel == _levelsConfig.MaxSteps
                        ? Levels.LevelState.GameCompleted
                        : Levels.LevelState.LevelCompleted;

                    return;
                }

                if (isPlayerDead)
                {
                    UpdateLevelScore(_currentLevel - 1);
                    _levelState.Value = Levels.LevelState.GameOver;
                    return;
                }

                _levelState.Value = Levels.LevelState.Playing;
            }

            void UpdateLevelScore(int value)
            {
                var savedScore = _scoreSave.Score.Value;
                if (value > savedScore)
                    _scoreSave.SaveScore(value);

                _levelScore.Value = value;
            }
        }

        public async UniTask StartNextLevel()
        {
            _enemySpawn.GridSize += _levelsConfig.GridStep;
            await _restartableLevel.Restart();
            _currentLevel++;
        }

        public void Dispose() => _subscriptions.Dispose();
    }
}