using Cysharp.Threading.Tasks;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace SwarmsOfGhosts.App.MainMenu
{
    public class MainMenuView : MonoBehaviour
    {
        [SerializeField] private GameObject _player;
        [SerializeField] private GameObject _enemy;
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

            _settingsButton.onClick.AddListener(() =>
            {
                EnableCharacters(false);

                _viewModel.OpenSettingsMenu()
                    .ContinueWith(() => EnableCharacters(true))
                    .Forget(e => { Debug.LogException(e); });
            });

            void EnableCharacters(bool enable)
            {
                _player.SetActive(enable);
                _enemy.SetActive(enable);
            }

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