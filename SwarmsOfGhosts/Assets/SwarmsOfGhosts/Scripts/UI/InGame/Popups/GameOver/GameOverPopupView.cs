﻿using System;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace SwarmsOfGhosts.UI.InGame.Popups.GameOver
{
    public class GameOverPopupView : MonoBehaviour
    {
        [SerializeField] private GameObject _contents;
        [SerializeField] private TextMeshProUGUI _scoreLabel;
        [SerializeField] private Button _quitButton;
        [Space]
        [SerializeField] private float _popupDelaySeconds = 0.4f;

        private string _scorePrefix;

        private IGameOverPopupViewModel _viewModel;

        [Inject]
        private void Construct(IGameOverPopupViewModel viewModel) => _viewModel = viewModel;

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

            _viewModel.IsGameOver
                .Where(value => value)
                .Delay(TimeSpan.FromSeconds(_popupDelaySeconds))
                .Subscribe(value => _contents.SetActive(value))
                .AddTo(this);

            _viewModel.IsGameOver
                .Where(value => !value)
                .Subscribe(value => _contents.SetActive(value))
                .AddTo(this);
        }

        private void OnDestroy() => _quitButton.onClick.RemoveAllListeners();
    }
}