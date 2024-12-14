using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameStateMachine : MonoBehaviour
{
    private interface IGameState
    {
        void Enter(GameStateMachine sm);
        void Exit(GameStateMachine sm);
    }

    private IGameState _currentState;
    private PlayerInput _playerInput;
    private PlayingState _playingState = new();
    private MenuState _menuState = new();

    private void Start()
    {
        _playerInput = PlayerCondition.Instance.GetComponent<PlayerInput>();
        TransitionToState(_playingState);
    }

    private void OnToggleMenu()
    {
        if (_currentState is PlayingState)
        {
            TransitionToState(_menuState);
        }
        else
        {
            TransitionToState(_playingState);
        }
    }

    private void TransitionToState(IGameState newState)
    {
        if (newState == null)
        {
            Debug.LogError("Attempting to transition to a null state!");
            return;
        }

        _currentState?.Exit(this); 
        _currentState = newState;  
        _currentState.Enter(this); 
    }

    private class MenuState : IGameState
    {
        public void Enter(GameStateMachine sm)
        {
            SceneManager.LoadScene("GameMenu", LoadSceneMode.Additive);
            sm._playerInput.SwitchCurrentActionMap("Menu");
        }

        public void Exit(GameStateMachine sm)
        {
            SceneManager.UnloadSceneAsync("GameMenu");
        }
    }

    private class PlayingState : IGameState
    {
        public void Enter(GameStateMachine sm)
        {
            sm._playerInput.SwitchCurrentActionMap("Player");
        }

        public void Exit(GameStateMachine sm)
        {
            // do nothing
        }
    }
}