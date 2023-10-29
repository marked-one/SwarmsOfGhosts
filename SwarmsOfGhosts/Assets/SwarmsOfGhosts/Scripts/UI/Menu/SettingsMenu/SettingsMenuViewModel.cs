using System;
using Cysharp.Threading.Tasks;
using SwarmsOfGhosts.App;
using SwarmsOfGhosts.UI.Input;
using UniRx;
using Zenject;

namespace SwarmsOfGhosts.UI.Menu.SettingsMenu
{
    public interface ISettingsMenuViewModel
    {
        public IReadOnlyReactiveProperty<bool> IsVisible { get; }
        public void Close();
    }

    public interface ISettingsMenu
    {
        public UniTask Open();
    }

    public class SettingsMenuViewModel : ISettingsMenuViewModel, ISettingsMenu, IDisposable
    {
        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();

        private UniTaskCompletionSource _menuOpenCompletionSource;

        private readonly ReactiveProperty<bool> _isVisible = new ReactiveProperty<bool>();
        public IReadOnlyReactiveProperty<bool> IsVisible => _isVisible;

        [Inject]
        private SettingsMenuViewModel(IMenuInput menuInput)
        {
            menuInput.Back
                .Subscribe(_ => _menuOpenCompletionSource?.TrySetResult())
                .AddTo(_subscriptions);
        }

        public async UniTask Open()
        {
            _isVisible.Value = true;
            _menuOpenCompletionSource = new UniTaskCompletionSource();
            await _menuOpenCompletionSource.Task;
            _menuOpenCompletionSource = null;
            _isVisible.Value = false;
        }

        public void Close() => _menuOpenCompletionSource?.TrySetResult();
        public void Dispose() => _subscriptions.Dispose();
    }
}