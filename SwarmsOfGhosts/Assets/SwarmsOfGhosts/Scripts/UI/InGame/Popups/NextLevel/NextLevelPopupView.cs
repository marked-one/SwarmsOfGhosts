using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace SwarmsOfGhosts.UI.InGame.Popups.NextLevel
{
    public class NextLevelPopupView : MonoBehaviour
    {
        [SerializeField] private GameObject _contents;
        [SerializeField] private TextMeshProUGUI _scoreLabel;
        [SerializeField] private Button _nextButton;
        [SerializeField] private Button _quitButton;
        [Space]
        [SerializeField] private float _popupDelaySeconds = 0.4f;

        private string _scorePrefix;

        private INextLevelPopupViewModel _viewModel;

        [Inject]
        private void Construct(INextLevelPopupViewModel popupViewModel) => _viewModel = popupViewModel;

        private void Awake()
        {
            _contents.SetActive(false);
            _scorePrefix = _scoreLabel.text;
        }

        private void Start()
        {
            _viewModel.LevelScore
                .Subscribe(value => _scoreLabel.text = $"{_scorePrefix}{value}")
                .AddTo(this);

            _viewModel.IsLevelWon
                .Where(value => value)
                .Delay(TimeSpan.FromSeconds(_popupDelaySeconds))
                .Subscribe(value => _contents.SetActive(value))
                .AddTo(this);

            _viewModel.IsLevelWon
                .Where(value => !value)
                .Subscribe(value => _contents.SetActive(value))
                .AddTo(this);

            _nextButton.onClick.AddListener(() =>
            {
                _nextButton.enabled = false;
                _viewModel
                    .StartNextLevel()
                    .ContinueWith(() => _nextButton.enabled = true)
                    .Forget(Debug.LogException);
            });

            _quitButton.onClick.AddListener(() =>
            {
                // TODO: Go to main menu
            });
        }

        private void OnDestroy()
        {
            _nextButton.onClick.RemoveAllListeners();
            _quitButton.onClick.RemoveAllListeners();
        }
    }
}