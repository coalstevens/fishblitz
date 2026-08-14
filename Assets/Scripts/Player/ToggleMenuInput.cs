using UnityEngine;
using UnityEngine.InputSystem;

public class ToggleMenuInput : MonoBehaviour
{
    [SerializeField] private GameObject _gameMenu;
    [SerializeField] private PlayerInput _playerInput;
    private bool _isMenuOpen = false;

    private void Start()
    {
        if (_gameMenu.activeSelf)
        {
            _gameMenu.SetActive(false);
            _playerInput?.SwitchCurrentActionMap("Player");
            Debug.Log("ToggleMenuInput: Closed active game menu and set action map to Player on start.");
        }

        _isMenuOpen = false;
    }

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
            CloseMenu();
        }
    }

    public void CloseMenu()
    {
        if (!_isMenuOpen)
            return;

        _isMenuOpen = false;
        _gameMenu.SetActive(false);
        _playerInput?.SwitchCurrentActionMap("Player");
    }
}
