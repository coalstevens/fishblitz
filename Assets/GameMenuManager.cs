using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameMenuManager : MonoBehaviour
{
    private bool isMenuLoaded = false;
    PlayerInput _playerInput;
    private void Start() {
        _playerInput = PlayerCondition.Instance.GetComponent<PlayerInput>();
    }
    private void OnToggleMenu()
    {
        if (!isMenuLoaded)
        {
            SceneManager.LoadScene("GameMenu", LoadSceneMode.Additive);
            isMenuLoaded = true;
            _playerInput.SwitchCurrentActionMap("Menu");
        }
        else
        {
            SceneManager.UnloadSceneAsync("GameMenu");
            isMenuLoaded = false;
            _playerInput.SwitchCurrentActionMap("Player");
        }
    }
}
