using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public static class GameStateManager
{
    private interface IGameState
    {
        void Enter();
        void Exit();
    }

    private static IGameState _currentState;
    private static PlayerInput _playerInput;
    private static Playing _playingState = new();
    private static NarratorOnBlack _narratorOnBlack = new();
    private static Scene _rootScene;
    private static bool _isMenuOpen = false;

    public static void Initialize()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
        Application.quitting += CleanUp;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Additive) return;
        _rootScene = scene;

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            _playerInput = player.GetComponent<PlayerInput>();

        TransitionToState(GetStateForScene(scene.name));
    }

    private static IGameState GetStateForScene(string sceneName)
    {
        return sceneName switch
        {
            "Boot" => _narratorOnBlack,
            "SleepMenu" => _narratorOnBlack,
            "Outside" => _playingState,
            "Abandoned Shed" => _playingState,
            _ => throw new System.IndexOutOfRangeException("Scene not recognized")
        };
    }

    private static void OnSceneUnloaded(Scene scene)
    {
        if (scene == _rootScene)
            _currentState?.Exit();
    }

    public static void OnToggleMenu()
    {
        if (_currentState is not Playing) return;
        if (!_isMenuOpen)
            OpenMenu();
        else
            CloseMenu();
    }

    private static void TransitionToState(IGameState newState)
    {
        if (newState == null)
        {
            Debug.LogError("Attempting to transition to a null state!");
            return;
        }
        _currentState = newState;
        _currentState.Enter();
    }

    private static void OpenMenu()
    {
        _isMenuOpen = true;
        SceneManager.LoadScene("GameMenu", LoadSceneMode.Additive);
        _playerInput?.SwitchCurrentActionMap("Menu");
    }

    private static void CloseMenu()
    {
        SceneManager.UnloadSceneAsync("GameMenu");
        _isMenuOpen = false;
        _playerInput?.SwitchCurrentActionMap("Player");
    }

    private class Playing : IGameState
    {
        public void Enter()
        {
            _playerInput?.SwitchCurrentActionMap("Player");
            SceneManager.LoadScene("Narrator", LoadSceneMode.Additive);
            SceneManager.LoadScene("HUD", LoadSceneMode.Additive);
        }

        public void Exit()
        {
            SceneManager.UnloadSceneAsync("Narrator");
            SceneManager.UnloadSceneAsync("HUD");
        }
    }

    private class NarratorOnBlack : IGameState
    {
        public void Enter()
        {
            SceneManager.LoadScene("Narrator", LoadSceneMode.Additive);
        }

        public void Exit()
        {
            SceneManager.UnloadSceneAsync("Narrator");
        }
    }
    private static void CleanUp()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        Application.quitting -= CleanUp;
    }
}