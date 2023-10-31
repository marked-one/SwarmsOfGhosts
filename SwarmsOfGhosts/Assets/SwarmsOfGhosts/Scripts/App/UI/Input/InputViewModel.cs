using System;
using UniRx;
using UnityEngine;

namespace SwarmsOfGhosts.App.UI.Input
{
    public interface IInputViewModel
    {
        public void SetMovement(Vector2 movement);
        public void BackPressed();
    }

    public interface IMenuInput
    {
        public IObservable<Unit> Back { get; }
    }

    public interface IGameplayInput
    {
        public IObservable<Vector2> Movement { get; }
    }

    public class InputViewModel : IInputViewModel, IMenuInput, IGameplayInput
    {
        private readonly Subject<Unit> _back = new Subject<Unit>();
        public IObservable<Unit> Back => _back;

        private readonly Subject<Vector2> _movement = new Subject<Vector2>();
        public IObservable<Vector2> Movement => _movement;

        public void SetMovement(Vector2 movement) => _movement.OnNext(movement);

        public void BackPressed() => _back.OnNext(Unit.Default);
    }
}