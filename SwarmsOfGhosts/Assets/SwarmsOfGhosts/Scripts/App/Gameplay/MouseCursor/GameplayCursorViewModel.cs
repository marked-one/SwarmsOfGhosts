using System;
using UniRx;
using Zenject;

namespace SwarmsOfGhosts.App.Gameplay.MouseCursor
{
    public interface IGameplayCursorViewModel
    {
        public IReadOnlyReactiveProperty<bool> IsVisible { get; }
    }

    public interface ICursor
    {
        public void SetVisibility(bool visible);
    }

    public class GameplayCursorViewModel : IGameplayCursorViewModel, ICursor, IInitializable, IDisposable
    {
        private readonly ReactiveProperty<bool> _isVisible = new ReactiveProperty<bool>();
        public IReadOnlyReactiveProperty<bool> IsVisible => _isVisible;

        public void Initialize() => _isVisible.Value = false;
        public void SetVisibility(bool isVisible) => _isVisible.Value = isVisible;
        public void Dispose() => _isVisible.Value = true;
    }
}