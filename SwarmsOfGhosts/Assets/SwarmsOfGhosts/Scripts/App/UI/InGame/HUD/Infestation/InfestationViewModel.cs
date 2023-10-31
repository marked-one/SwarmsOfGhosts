using SwarmsOfGhosts.App.Gameplay;
using UniRx;
using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.App.UI.InGame.HUD.Infestation
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

            infestation.MaxInfestation
                .Subscribe(value => maxInfestation = value)
                .AddTo(_subscriptions);

            infestation.CurrentInfestation
                .Subscribe(value =>
                {
                    _infestation.Value = Mathf.Approximately(maxInfestation, 0f) ? 1f : value / maxInfestation;
                })
                .AddTo(_subscriptions);
        }
    }
}