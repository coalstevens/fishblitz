using UnityEngine;

public class ToggleMenuInput : MonoBehaviour
{
    [SerializeField] private GameObject _gameMenu;
    private bool _isMenuOpen = false;

    private void Start()
    {
        if (_gameMenu.activeSelf)
        {
            _gameMenu.SetActive(false);
            PlayerActionMapController.Instance.PopOverlay(PlayerActionMapController.Overlay.Menu);
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
            PlayerActionMapController.Instance.PushOverlay(PlayerActionMapController.Overlay.Menu);
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
        PlayerActionMapController.Instance.PopOverlay(PlayerActionMapController.Overlay.Menu);
    }
}
