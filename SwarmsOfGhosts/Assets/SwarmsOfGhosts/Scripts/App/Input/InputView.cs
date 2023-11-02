using System;
using InputSystem;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace SwarmsOfGhosts.App.Input
{
    public class InputView : IInitializable, ITickable, IDisposable
    {
        private InputActions _inputActions;
        private readonly IInputViewModel _viewModel;

        [Inject]
        private InputView(IInputViewModel viewModel) => _viewModel = viewModel;

        public void Initialize()
        {
            _inputActions = new InputActions();
            _inputActions.Enable();

            // It is good to have Esc key as Back. But Esc
            // key changes cursor visibility in Unity Editor.
            // So it is better to use some other key there.

#if UNITY_EDITOR
            _inputActions.Keyboard.EditorBack.performed += OnKeyboardBackKeyPressed;
#else
            _inputActions.Keyboard.Back.performed += OnKeyboardBackKeyPressed;
#endif

            _inputActions.Keyboard.FPS.performed += OnKeyboardFpsToggleKeyPressed;
        }

        public void Dispose()
        {
#if UNITY_EDITOR
            _inputActions.Keyboard.EditorBack.performed -= OnKeyboardBackKeyPressed;
#else
            _inputActions.Keyboard.Back.performed -= OnKeyboardBackKeyPressed;
#endif

            _inputActions.Keyboard.FPS.performed -= OnKeyboardFpsToggleKeyPressed;
            _inputActions.Disable();
        }

        private void OnKeyboardBackKeyPressed(InputAction.CallbackContext context) => _viewModel.BackPressed();
        private void OnKeyboardFpsToggleKeyPressed(InputAction.CallbackContext context) => _viewModel.FpsToggled();

        public void Tick()
        {
            var movement = _inputActions.Keyboard.WASD.ReadValue<Vector2>();
            _viewModel.SetMovement(movement);
        }
    }
}