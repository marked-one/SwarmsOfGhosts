using TMPro;
using UniRx;
using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.App.Fps
{
    public class FpsCounterView : MonoBehaviour
    {
        [SerializeField] private GameObject _contents;
        [SerializeField] private TextMeshProUGUI _fpsLabel;

        private IFpsCounterViewModel _viewModel;

        [Inject]
        private void Construct(IFpsCounterViewModel viewModel) => _viewModel = viewModel;

        private void Start()
        {
            _viewModel.IsVisible
                .Subscribe(_contents.SetActive)
                .AddTo(this);

            _viewModel.Fps
                .Subscribe(value => _fpsLabel.text = value.ToString("0.00"))
                .AddTo(this);
        }
    }
}