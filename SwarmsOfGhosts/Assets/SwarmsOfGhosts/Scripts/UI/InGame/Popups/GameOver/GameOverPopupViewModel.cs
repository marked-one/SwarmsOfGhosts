using SwarmsOfGhosts.MetaGame.Levels;
using UniRx;
using Zenject;

namespace SwarmsOfGhosts.UI.InGame.Popups.GameOver
{
    public interface IGameOverPopupViewModel
    {
        public IReadOnlyReactiveProperty<int> LevelScore { get; }
        public IReadOnlyReactiveProperty<bool> IsGameOver { get; }
    }

    public class GameOverPopupViewModel : IGameOverPopupViewModel
    {
        public IReadOnlyReactiveProperty<int> LevelScore { get; }
        public IReadOnlyReactiveProperty<bool> IsGameOver { get; }

        [Inject]
        private GameOverPopupViewModel(ILevelSwitcher levelSwitcher)
        {
            LevelScore = levelSwitcher.LevelScore;
            IsGameOver = levelSwitcher.IsGameOver;
        }
    }
}