using System;
using UniRx;
using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.App.UI.InGame.GameplayCursor
{
    public class GameplayCursorView : IInitializable, IDisposable
    {
        private readonly IGameplayCursorViewModel _viewModel;

        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();

        private GameplayCursorView(IGameplayCursorViewModel viewModel) => _viewModel = viewModel;

        public void Initialize() =>
            _viewModel.IsVisible
                .Subscribe(value => Cursor.visible = value)
                .AddTo(_subscriptions);

        public void Dispose() => _subscriptions.Dispose();
    }
}