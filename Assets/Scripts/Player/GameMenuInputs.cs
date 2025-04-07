using UnityEngine;
using UnityEngine.InputSystem;

public class GameMenuInputs : MonoBehaviour
{
    private GameMenuManager _gameMenu;

    private void OnEnable()
    {
        _gameMenu = FindFirstObjectByType<GameMenuManager>();        
    }

    private void OnMoveCursor(InputValue value)
    {
        if (value.Get<Vector2>() == Vector2.zero)
            return;

        if (_gameMenu == null)
            _gameMenu = FindFirstObjectByType<GameMenuManager>();

        _gameMenu.OnMoveCursor(value);
    }

    private void OnSelect()
    {
        if (_gameMenu == null)
            _gameMenu = FindFirstObjectByType<GameMenuManager>();   
        _gameMenu.OnSelect();
    }
}
