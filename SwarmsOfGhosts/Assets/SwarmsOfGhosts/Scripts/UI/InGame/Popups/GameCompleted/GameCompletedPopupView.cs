using System;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace SwarmsOfGhosts.UI.InGame.Popups.GameCompleted
{
    public class GameCompletedPopupView : MonoBehaviour
    {
        [SerializeField] private GameObject _contents;
        [SerializeField] private TextMeshProUGUI _scoreLabel;
        [SerializeField] private Button _quitButton;
        [Space]
        [SerializeField] private float _popupDelaySeconds = 0.4f;

        private string _scorePrefix;

        private IGameCompletedPopupViewModel _viewModel;

        [Inject]
        private void Construct(IGameCompletedPopupViewModel viewModel) => _viewModel = viewModel;

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

            _viewModel.IsVisible
                .Where(value => value)
                .Delay(TimeSpan.FromSeconds(_popupDelaySeconds))
                .Subscribe(SetVisible)
                .AddTo(this);

            _viewModel.IsVisible
                .Where(value => !value)
                .Subscribe(SetVisible)
                .AddTo(this);

            void SetVisible(bool isVisible)
            {
                _contents.SetActive(isVisible);
                _viewModel.SetCursorVisible(isVisible);
            }

            _quitButton.onClick.AddListener(_viewModel.OpenMainMenuScene);
        }

        private void OnDestroy() => _quitButton.onClick.RemoveAllListeners();
    }
}