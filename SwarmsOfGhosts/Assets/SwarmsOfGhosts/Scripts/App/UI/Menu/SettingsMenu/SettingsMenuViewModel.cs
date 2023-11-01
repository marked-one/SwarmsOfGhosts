using System;
using Cysharp.Threading.Tasks;
using SwarmsOfGhosts.App.UI.Input;
using UniRx;
using Zenject;

namespace SwarmsOfGhosts.App.UI.Menu.SettingsMenu
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

        private UniTaskCompletionSource _settingsMenuCompletionSource;

        private readonly ReactiveProperty<bool> _isVisible = new ReactiveProperty<bool>();
        public IReadOnlyReactiveProperty<bool> IsVisible => _isVisible;

        [Inject]
        private SettingsMenuViewModel(IMenuInput menuInput)
        {
            menuInput.Back
                .Subscribe(_ => _settingsMenuCompletionSource?.TrySetResult())
                .AddTo(_subscriptions);
        }

        public async UniTask Open()
        {
            _isVisible.Value = true;
            _settingsMenuCompletionSource = new UniTaskCompletionSource();
            await _settingsMenuCompletionSource.Task;
            _settingsMenuCompletionSource = null;
            _isVisible.Value = false;
        }

        public void Close() => _settingsMenuCompletionSource?.TrySetResult();
        public void Dispose() => _subscriptions.Dispose();
    }
}