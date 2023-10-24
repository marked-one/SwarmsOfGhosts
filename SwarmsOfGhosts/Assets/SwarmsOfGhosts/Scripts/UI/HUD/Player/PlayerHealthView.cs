using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace SwarmsOfGhosts.UI.HUD.Player
{
    public class PlayerHealthView : MonoBehaviour
    {
        [SerializeField] private Slider _healthBar;

        private IPlayerHealthViewModel _viewModel;

        [Inject]
        private void Construct(IPlayerHealthViewModel viewModel) => _viewModel = viewModel;

        private void Start()
        {
            _viewModel.PlayerHealth
                .Subscribe(value => { _healthBar.value = value; })
                .AddTo(this);
        }
    }
}