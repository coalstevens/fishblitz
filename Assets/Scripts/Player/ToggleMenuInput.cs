using UnityEngine;
using UnityEngine.InputSystem;

public class ToggleMenuInput : MonoBehaviour
{
    [SerializeField] private GameObject _gameMenu;
    [SerializeField] private PlayerInput _playerInput;
    private bool _isMenuOpen = false;

    private void OnToggleMenu()
    {
        if (_gameMenu == null)
            return;

        if (!_isMenuOpen)
        {
            _isMenuOpen = true;
            _gameMenu.SetActive(true);
            _playerInput?.SwitchCurrentActionMap("Menu");
        }
        else
        {
            _isMenuOpen = false;
            _gameMenu.SetActive(false);
            _playerInput?.SwitchCurrentActionMap("Player");
        }
    }
}
