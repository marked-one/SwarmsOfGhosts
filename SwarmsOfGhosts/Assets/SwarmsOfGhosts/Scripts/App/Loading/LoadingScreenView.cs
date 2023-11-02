using UniRx;
using UnityEngine;
using Zenject;

namespace SwarmsOfGhosts.App.Loading
{
    public class LoadingScreenView : MonoBehaviour
    {
        [SerializeField] private GameObject _contents;

        private ILoadingScreenViewModel _viewModel;

        [Inject]
        private void Construct(ILoadingScreenViewModel viewModel) => _viewModel = viewModel;

        public void Awake() => _contents.SetActive(false);

        private void Start() =>
            _viewModel.IsVisible
                .Subscribe(_contents.SetActive)
                .AddTo(this);
    }
}