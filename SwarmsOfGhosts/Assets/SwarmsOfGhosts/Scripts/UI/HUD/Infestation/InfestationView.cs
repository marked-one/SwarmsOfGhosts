using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace SwarmsOfGhosts.UI.HUD.Infestation
{
    public class InfestationView : MonoBehaviour
    {
        [SerializeField] private Slider _infestationBar;

        private IInfestationViewModel _viewModel;

        [Inject]
        private void Construct(IInfestationViewModel viewModel) => _viewModel = viewModel;

        private void Start()
        {
            _viewModel.Infestation
                .Subscribe(value => { _infestationBar.value = value; })
                .AddTo(this);
        }
    }
}