using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace SwarmsOfGhosts.UI.Menu.MainMenu
{
    public class MainMenuView : MonoBehaviour
    {
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _quitButton;
        [SerializeField] private TextMeshProUGUI _bestScoreLabel;

        private string _scorePrefix;

        private IMainMenuViewModel _viewModel;

        [Inject]
        private void Construct(IMainMenuViewModel viewModel) => _viewModel = viewModel;

        private void Awake() => _scorePrefix = _bestScoreLabel.text;

        private void Start()
        {
            _viewModel.BestScore
                .Subscribe(value => _bestScoreLabel.text = $"{_scorePrefix}{value}")
                .AddTo(this);

            _playButton.onClick.AddListener(_viewModel.OpenGameplayScene);
            _settingsButton.onClick.AddListener(_viewModel.OpenSettingsMenu);
            _quitButton.onClick.AddListener(_viewModel.QuitApplication);
        }

        private void OnDestroy()
        {
            _playButton.onClick.RemoveAllListeners();
            _settingsButton.onClick.RemoveAllListeners();
            _quitButton.onClick.RemoveAllListeners();
        }
    }
}