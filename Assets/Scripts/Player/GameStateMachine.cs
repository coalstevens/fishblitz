using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public static class GameStateManager {
    private interface IGameState
    {
        void Enter();
        void Exit();
    }

    private static IGameState _currentState;
    private static PlayerInput _playerInput;
    private static Playing _playingState = new();
    private static MenuOpen _menuState = new();
    private static NarratorOnBlack _narratorOnBlack = new();
    private static Scene _rootScene;

    public static void Initialize() {
        Debug.Log("Gamestate manager actived");
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Gamestatemanager sceneloaded");
        if (mode == LoadSceneMode.Additive) return;
        _rootScene = scene;

        IGameState newState = scene.name switch {
            "Boot" => _narratorOnBlack,
            "Outside" => _playingState,
            "Abandonded Shed" => _playingState,
            _ => null,
        };
        if (newState == null)
            Debug.LogError("Unhandled scene in GameStateManager");
        TransitionToState(newState);
    }

    private static void OnSceneUnloaded(Scene scene)
    {
        if (scene == _rootScene)
            _currentState?.Exit();
    }

    public static void OnToggleMenu()
    {
        if (_currentState is Playing)
        {
            TransitionToState(_menuState);
        }
        else
        {
            TransitionToState(_playingState);
        }
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

    private class MenuOpen : IGameState
    {
        public void Enter()
        {
            SceneManager.LoadScene("GameMenu", LoadSceneMode.Additive);
            if (PlayerCondition.Instance != null)
                PlayerCondition.Instance.GetComponent<PlayerInput>().SwitchCurrentActionMap("Menu");
        }

        public void Exit()
        {
            SceneManager.UnloadSceneAsync("GameMenu");
        }
    }

    private class Playing : IGameState
    {
        public void Enter()
        {
            if (PlayerCondition.Instance != null)
                PlayerCondition.Instance.GetComponent<PlayerInput>().SwitchCurrentActionMap("Player");
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
}