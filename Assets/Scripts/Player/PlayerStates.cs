public enum PlayerStates
{
    Running,
    Idle,
    Fishing,
    Axing,
    Catching,
    Celebrating,
    Birding,
    BirdingRunning,
    PickingUp,
    Crouched,
    BowCharging,
    BowChargingRunning,
    Barrowing,
    BarrowingRunning
}

public static class PlayerStateCapabilities
{
    public static bool CanMove(this PlayerStates state) =>
        state is PlayerStates.Idle or PlayerStates.Running or PlayerStates.Birding
            or PlayerStates.BirdingRunning or PlayerStates.BowCharging
            or PlayerStates.BowChargingRunning or PlayerStates.Barrowing
            or PlayerStates.BarrowingRunning;

    public static bool CanInteract(this PlayerStates state) => state is PlayerStates.Idle or PlayerStates.Running;

    public static bool CanCrouch(this PlayerStates state) => state is PlayerStates.Idle or PlayerStates.Running;

    public static bool IsBowCharging(this PlayerStates state) => state is PlayerStates.BowCharging or PlayerStates.BowChargingRunning;

    public static bool CanUseItem(this PlayerStates state) =>
        state is not (PlayerStates.Celebrating or PlayerStates.Catching or PlayerStates.Axing
            or PlayerStates.PickingUp or PlayerStates.Birding or PlayerStates.BirdingRunning
            or PlayerStates.BowCharging or PlayerStates.BowChargingRunning
            or PlayerStates.Barrowing or PlayerStates.BarrowingRunning);
}