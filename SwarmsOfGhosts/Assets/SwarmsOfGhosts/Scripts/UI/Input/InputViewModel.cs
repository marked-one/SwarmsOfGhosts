using System;
using UniRx;
using UnityEngine;

namespace SwarmsOfGhosts.UI.Input
{
    public interface IInputViewModel
    {
        public Vector2 Movement { set; }
        public void BackPressed();
    }

    public interface IMenuInput
    {
        public IObservable<Unit> Back { get; }
    }

    public interface IPlayerInput
    {
        public Vector2 Movement { get; }
    }

    public class InputViewModel : IInputViewModel, IMenuInput, IPlayerInput
    {
        private readonly Subject<Unit> _back = new Subject<Unit>();
        public IObservable<Unit> Back => _back;

        public Vector2 Movement { get; set; }

        public void BackPressed() => _back.OnNext(Unit.Default);
    }
}