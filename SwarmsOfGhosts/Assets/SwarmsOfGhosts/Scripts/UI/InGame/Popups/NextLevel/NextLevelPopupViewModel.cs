using Cysharp.Threading.Tasks;
using SwarmsOfGhosts.MetaGame.Levels;
using UniRx;
using Zenject;

namespace SwarmsOfGhosts.UI.InGame.Popups.NextLevel
{
    public interface INextLevelPopupViewModel
    {
        public IReadOnlyReactiveProperty<int> LevelScore { get; }
        public IReadOnlyReactiveProperty<bool> IsLevelWon { get; }
        public UniTask StartNextLevel();
    }

    public class NextLevelPopupViewModel : INextLevelPopupViewModel
    {
        private readonly ILevelSwitcher _levelSwitcher;

        public IReadOnlyReactiveProperty<int> LevelScore { get; }
        public IReadOnlyReactiveProperty<bool> IsLevelWon { get; }

        [Inject]
        private NextLevelPopupViewModel(ILevelSwitcher levelSwitcher)
        {
            _levelSwitcher = levelSwitcher;
            LevelScore = levelSwitcher.LevelScore;
            IsLevelWon = levelSwitcher.IsLevelWon;
        }

        public async UniTask StartNextLevel()
        {
            await _levelSwitcher.StartNextLevel();
        }
    }
}