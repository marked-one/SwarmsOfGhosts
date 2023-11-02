using Debugging;
using SwarmsOfGhosts.App.Input;
using UniRx;
using Zenject;

namespace SwarmsOfGhosts.App.Fps
{
    public interface IFpsCounterViewModel
    {
        public IReadOnlyReactiveProperty<bool> IsVisible { get; }
        public IReadOnlyReactiveProperty<float> Fps { get; }
    }

    public class FpsCounterViewModel : IFpsCounterViewModel
    {
        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();

        private readonly ReactiveProperty<bool> _isVisible = new ReactiveProperty<bool>();
        public IReadOnlyReactiveProperty<bool> IsVisible => _isVisible;

        public IReadOnlyReactiveProperty<float> Fps { get; }

        [Inject]
        private FpsCounterViewModel(IFpsCounter fpsCounter, IDebugInput menuInput)
        {
            Fps = fpsCounter.Fps;

            menuInput.FpsToggle
                .Subscribe(_ => _isVisible.Value = !_isVisible.Value)
                .AddTo(_subscriptions);
        }
    }
}