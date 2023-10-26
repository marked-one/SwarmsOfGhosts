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
    public interface ILevelSwitcher
    {
        public IReadOnlyReactiveProperty<int> LevelScore { get; }
        public IReadOnlyReactiveProperty<bool> IsLevelWon { get; }
        public IReadOnlyReactiveProperty<bool> IsGameOver { get; }
        public UniTask StartNextLevel();
    }

    public class LevelSwitcher : ILevelSwitcher, IInitializable, IDisposable
    {
        private readonly ISave _scoreSave;
        private readonly IRestartable _restartableLevel;
        private readonly IInfestation _enemyInfestation;
        private readonly IPlayerHealth _playerHealth;

        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();

        private int _currentLevel;

        private readonly ReactiveProperty<int> _levelScore = new ReactiveProperty<int>(0);
        public IReadOnlyReactiveProperty<int> LevelScore => _levelScore;

        private readonly ReactiveProperty<bool> _isLevelWon = new ReactiveProperty<bool>();
        public IReadOnlyReactiveProperty<bool> IsLevelWon => _isLevelWon;

        private readonly ReactiveProperty<bool> _isGameOver = new ReactiveProperty<bool>();
        public IReadOnlyReactiveProperty<bool> IsGameOver => _isGameOver;

        [Inject]
        private LevelSwitcher(ISave scoreSave, IRestartable restartableLevel,
            IInfestation enemyInfestation, IPlayerHealth playerHealth)
        {
            _scoreSave = scoreSave;
            _restartableLevel = restartableLevel;
            _enemyInfestation = enemyInfestation;
            _playerHealth = playerHealth;
        }

        public void Initialize()
        {
            _currentLevel = 1;
            _isLevelWon.Value = false;

            _enemyInfestation.Current
                .Skip(1) // Skip initialization because value is 0 in the beginning.
                .Subscribe(value => EndLevel(value, _isLevelWon, _currentLevel))
                .AddTo(_subscriptions);

            _playerHealth.Current
                .Skip(1) // Skip initialization because value is 0 in the beginning.
                .Subscribe(value => EndLevel(value, _isGameOver, _currentLevel - 1))
                .AddTo(_subscriptions);

            void EndLevel(float value, ReactiveProperty<bool> needShowPopup, int currentLevel)
            {
                if (Mathf.Approximately(value, 0f))
                {
                    UpdateLevelScore(currentLevel);
                    needShowPopup.Value = true;
                    return;
                }

                needShowPopup.Value = false;
            }
        }

        private void UpdateLevelScore(int value)
        {
            var savedScore = _scoreSave.Score.Value;
            if (value > savedScore)
                _scoreSave.SaveScore(value);

            _levelScore.Value = value;
        }

        public async UniTask StartNextLevel()
        {
            await _restartableLevel.Restart();
            _currentLevel++;
        }

        public void Dispose() => _subscriptions.Dispose();
    }
}