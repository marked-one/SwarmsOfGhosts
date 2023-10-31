using System;
using UniRx;
using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.App.Gameplay
{
    public class GameplayCamera : IDisposable
    {
        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();

        [Inject]
        private GameplayCamera(Camera camera, IPlayerPosition playerPosition) =>
            playerPosition.PlayerPosition
                .Subscribe(position => camera.transform.parent.position = position)
                .AddTo(_subscriptions);

        public void Dispose() => _subscriptions.Dispose();
    }
}