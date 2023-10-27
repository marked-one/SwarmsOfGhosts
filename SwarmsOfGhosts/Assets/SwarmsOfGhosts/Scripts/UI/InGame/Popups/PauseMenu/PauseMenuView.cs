using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace SwarmsOfGhosts.UI.InGame.Popups.PauseMenu
{
    public class PauseMenuView : MonoBehaviour
    {
        [SerializeField] private GameObject _contents;
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _quitButton;

        private IPauseMenuViewModel _viewModel;

        [Inject]
        private void Construct(IPauseMenuViewModel viewModel) => _viewModel = viewModel;

        private void Awake() => _contents.SetActive(false);

        private void Start()
        {
            _viewModel.IsPaused
                .Subscribe(isPaused => _contents.SetActive(isPaused))
                .AddTo(this);

            _continueButton.onClick.AddListener(_viewModel.Unpause);
            _settingsButton.onClick.AddListener(_viewModel.OpenSettingsMenu);
            _quitButton.onClick.AddListener(_viewModel.OpenMainMenuScene);
        }

        private void OnDestroy()
        {
            _continueButton.onClick.RemoveAllListeners();
            _settingsButton.onClick.RemoveAllListeners();
            _quitButton.onClick.RemoveAllListeners();
        }
    }
}