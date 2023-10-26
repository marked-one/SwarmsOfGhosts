using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace SwarmsOfGhosts.UI.HUD.PauseMenu
{
    public class PauseMenuView : MonoBehaviour
    {
        [SerializeField] private GameObject _pauseMenuContents;
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _quitButton;

        private IPauseMenuViewModel _viewModel;

        [Inject]
        private void Construct(IPauseMenuViewModel viewModel) => _viewModel = viewModel;

        private void Awake() => _pauseMenuContents.SetActive(false);

        private void Start()
        {
            _viewModel.IsPaused
                .Subscribe(isPaused => _pauseMenuContents.SetActive(isPaused))
                .AddTo(this);

            _continueButton.onClick.AddListener(() => _viewModel.Unpause());

            _settingsButton.onClick.AddListener(() =>
            {
                // TODO: Go to Settings menu.
            });

            _quitButton.onClick.AddListener(() =>
            {
                // TODO: Go to main menu
            });
        }

        private void OnDestroy()
        {
            _continueButton.onClick.RemoveAllListeners();
            _settingsButton.onClick.RemoveAllListeners();
            _quitButton.onClick.RemoveAllListeners();
        }
    }
}