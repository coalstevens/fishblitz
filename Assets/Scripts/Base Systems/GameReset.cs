using System.IO;
using UnityEngine;

public static class GameReset
{
    public static void ClearAllSaveFiles()
    {
        string[] files = Directory.GetFiles(Application.persistentDataPath);
        foreach (string file in files)
            File.Delete(file);
    }

    public static void ResetPlayerState(Inventory playerInventory = null)
    {
        var _energyManager = Object.FindFirstObjectByType<PlayerEnergyManager>();
        var _dryingManager = Object.FindFirstObjectByType<PlayerDryingManager>();
        var _temperatureManager = Object.FindFirstObjectByType<PlayerTemperatureManager>();
        var _strengthManager = Object.FindFirstObjectByType<PlayerStrength>();

        if (_energyManager != null) _energyManager.ResetToDefaults();
        if (_dryingManager != null) _dryingManager.ResetToDefaults();
        if (_temperatureManager != null) _temperatureManager.ResetToDefaults();
        if (_strengthManager != null) _strengthManager.ResetToDefaults();

        if (playerInventory != null)
            playerInventory.ActiveItemSlot.Value = 0;

        var _playerCarry = Object.FindFirstObjectByType<PlayerCarry>();
        if (_playerCarry != null)
        {
            _playerCarry.IsCarrying.Value = false;
            _playerCarry.CarriedStack.Clear();
        }

        var _playerWheelBarrow = Object.FindFirstObjectByType<PlayerWheelBarrow>();
        if (_playerWheelBarrow != null)
        {
            _playerWheelBarrow.IsHoldingWheelBarrow.Value = false;
            _playerWheelBarrow.WheelBarrowStack.Clear();
        }
    }

    public static void ResetClock()
    {
        if (GameClock.Instance != null)
            GameClock.Instance.ResetToStart();
    }
}
