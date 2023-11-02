using System;
using UniRx;
using UnityEngine;

namespace SwarmsOfGhosts.App.Input
{
    public interface IInputViewModel
    {
        public void SetMovement(Vector2 movement);
        public void BackPressed();
        public void FpsToggled();
    }

    public interface IMenuInput
    {
        public IObservable<Unit> Back { get; }
    }

    public interface IGameplayInput
    {
        public IObservable<Vector2> Movement { get; }
    }

    public interface IDebugInput
    {
        public IObservable<Unit> FpsToggle { get; }
    }

    public class InputViewModel : IInputViewModel, IMenuInput, IGameplayInput, IDebugInput
    {
        private readonly Subject<Unit> _back = new Subject<Unit>();
        public IObservable<Unit> Back => _back;

        private readonly Subject<Unit> _fpsToggle = new Subject<Unit>();
        public IObservable<Unit> FpsToggle => _fpsToggle;

        private readonly Subject<Vector2> _movement = new Subject<Vector2>();
        public IObservable<Vector2> Movement => _movement;

        public void SetMovement(Vector2 movement) => _movement.OnNext(movement);

        public void BackPressed() => _back.OnNext(Unit.Default);
        public void FpsToggled() => _fpsToggle.OnNext(Unit.Default);
    }
}