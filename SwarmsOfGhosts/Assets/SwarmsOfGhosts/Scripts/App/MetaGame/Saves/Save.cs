using UniRx;
using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.App.MetaGame.Saves
{
    public interface ISave
    {
        public IReadOnlyReactiveProperty<int> Score { get; }
        public void SaveScore(int score);
    }

    public class Save : ISave, IInitializable
    {
        private const string _scoreKey = "Score";

        private readonly ReactiveProperty<int> _score = new ReactiveProperty<int>();
        public IReadOnlyReactiveProperty<int> Score => _score;

        public void Initialize() => _score.Value = PlayerPrefs.GetInt(_scoreKey, 0);

        public void SaveScore(int score)
        {
            PlayerPrefs.SetInt(_scoreKey, score);
            _score.Value = score;
        }
    }
}