using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class ToggleMenuInput : MonoBehaviour
{
    [SerializeField] private GameObject _gameMenu;
    private PlayerInput _playerInput;
    private bool _isMenuOpen = false;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
    }

    private void Start()
    {
        if (_gameMenu.activeSelf)
        {
            _gameMenu.SetActive(false);
            SwitchToPlayerMap();
            Debug.Log("ToggleMenuInput: Closed active game menu on start.");
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
            SwitchToMenuMap();
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
        SwitchToPlayerMap();
    }

    private void SwitchToMenuMap() => SwitchToMap("Menu");

    private void SwitchToPlayerMap() => SwitchToMap("Player");

    private void SwitchToMap(string map)
    {
        if (_playerInput.currentActionMap?.name != map)
            _playerInput.SwitchCurrentActionMap(map);
    }
}