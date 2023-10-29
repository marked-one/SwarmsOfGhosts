using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace SwarmsOfGhosts.UI.Menu.SettingsMenu
{
    public class SettingsMenuView : MonoBehaviour
    {
        [SerializeField] private GameObject _contents;
        [SerializeField] private Button _quitButton;

        private ISettingsMenuViewModel _viewModel;

        [Inject]
        private void Construct(ISettingsMenuViewModel viewModel) => _viewModel = viewModel;

        public void Awake() => _contents.SetActive(false);

        private void Start()
        {
            _viewModel.IsVisible
                .Subscribe(_contents.SetActive)
                .AddTo(this);

            _quitButton.onClick.AddListener(_viewModel.Close);
        }

        private void OnDestroy() => _quitButton.onClick.RemoveAllListeners();
    }
}