using SwarmsOfGhosts.Gameplay.Pause;
using UniRx;
using Zenject;

namespace SwarmsOfGhosts.UI.InGame.Popups.PauseMenu
{
    public interface IPauseMenuViewModel
    {
        public IReadOnlyReactiveProperty<bool> IsPaused { get; }
        public void Unpause();
    }

    public class PauseMenuViewModel : IPauseMenuViewModel
    {
        private readonly IPausable _pausable;

        public IReadOnlyReactiveProperty<bool> IsPaused { get; }

        [Inject]
        private PauseMenuViewModel(IPausable pausable)
        {
            _pausable = pausable;
            IsPaused = _pausable.IsPaused;
        }

        public void Unpause() => _pausable.Unpause();
    }
}