using ReactiveUnity;
using UnityEngine;

public enum PlayerStates
{
    Default,
    Fishing,
    Axing,
    Catching,
    Celebrating,
    Birding,
    PickingUp,
    Crouched,
    BowCharging,
    Barrowing
}

public static class PlayerStateCapabilities
{
    public static bool CanMove(this PlayerStates state) =>
        state is PlayerStates.Default or PlayerStates.Birding or PlayerStates.BowCharging or PlayerStates.Barrowing;

    public static bool CanInteract(this PlayerStates state) => state is PlayerStates.Default;

    public static bool CanCrouch(this PlayerStates state) => state is PlayerStates.Default;

    public static bool IsBowCharging(this PlayerStates state) => state is PlayerStates.BowCharging;

    public static bool CanUseItem(this PlayerStates state) =>
        state is not (PlayerStates.Celebrating or PlayerStates.Catching or PlayerStates.Axing
            or PlayerStates.PickingUp or PlayerStates.Birding or PlayerStates.BowCharging
            or PlayerStates.Barrowing);
}

public class PlayerStateController : MonoBehaviour
{
    private static PlayerStateController _instance;
    public static PlayerStateController Instance
    {
        get
        {
            if (_instance == null)
                Debug.LogError("This object does not exist");
            return _instance;
        }
    }

    public Reactive<PlayerStates> PlayerState = new Reactive<PlayerStates>(PlayerStates.Default);
    public Reactive<bool> IsRunning = new Reactive<bool>(false);

    private PlayerStates _playerState = PlayerStates.Default;
    private bool _transitioning;

    private void Awake()
    {
        _instance = this;
    }

    public void TransitionTo(PlayerStates next)
    {
        if (_transitioning) return;
        if (next == _playerState) return;

        _transitioning = true;
        ExitState(_playerState);
        EnterState(next);
        _transitioning = false;
    }

    public void SetRunning(bool running)
    {
        IsRunning.Value = running;
    }

    public void ResetPlayerState()
    {
        TransitionTo(PlayerStates.Default);
    }

    private void EnterState(PlayerStates next)
    {
        switch (next)
        {
            // Enter side effects
        }

        _playerState = next;
        PlayerState.Value = next;
    }

    private void ExitState(PlayerStates current)
    {
        switch (current)
        {
            case PlayerStates.BowCharging:
                if (FindFirstObjectByType<BowChargeController>() is { } controller)
                    controller.AbortCharge();
                break;
        }
    }
}