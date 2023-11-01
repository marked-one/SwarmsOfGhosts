using UniRx;

namespace SwarmsOfGhosts.App.UI.LoadingScreen
{
    public interface ILoadingScreenViewModel
    {
        public IReadOnlyReactiveProperty<bool> IsVisible { get; }
    }

    public interface ILoadingScreen
    {
        public void Show();
        public void Hide();
    }

    public class LoadingScreenViewModel : ILoadingScreenViewModel, ILoadingScreen
    {
        private readonly ReactiveProperty<bool> _isVisible = new ReactiveProperty<bool>();
        public IReadOnlyReactiveProperty<bool> IsVisible => _isVisible;
        public void Show() => _isVisible.Value = true;
        public void Hide() => _isVisible.Value = false;
    }
}