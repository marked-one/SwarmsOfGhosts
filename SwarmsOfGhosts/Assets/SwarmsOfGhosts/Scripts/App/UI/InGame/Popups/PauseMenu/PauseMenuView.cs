using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace SwarmsOfGhosts.App.UI.InGame.Popups.PauseMenu
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
            _viewModel.IsVisible
                .Subscribe(SetVisible)
                .AddTo(this);

            void SetVisible(bool isVisible)
            {
                _contents.SetActive(isVisible);
                _viewModel.SetCursorVisible(isVisible);
            }

            _continueButton.onClick.AddListener(() => _viewModel.Close());

            _settingsButton.onClick.AddListener(() =>
                _viewModel
                    .OpenSettingsMenu()
                    .Forget(e => { Debug.LogException(e); }));

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