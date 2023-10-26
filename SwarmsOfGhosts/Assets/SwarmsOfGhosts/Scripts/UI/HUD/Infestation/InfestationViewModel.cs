﻿using SwarmsOfGhosts.Gameplay.Enemy;
using UniRx;
using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.UI.HUD.Infestation
{
    public interface IInfestationViewModel
    {
        public IReadOnlyReactiveProperty<float> Infestation { get; }
    }

    public class InfestationViewModel : IInfestationViewModel
    {
        private readonly ReactiveProperty<float> _infestation = new ReactiveProperty<float>();
        public IReadOnlyReactiveProperty<float> Infestation => _infestation;

        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();

        [Inject]
        private InfestationViewModel(IInfestation infestation)
        {
            var maxInfestation = 0f;
            infestation.Max
                .Subscribe(value => maxInfestation = value)
                .AddTo(_subscriptions);

            infestation.Current
                .Subscribe(value =>
                {
                    _infestation.Value = Mathf.Approximately(maxInfestation, 0f) ? 1f : value / maxInfestation;
                })
                .AddTo(_subscriptions);
        }
    }
}