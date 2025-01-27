using System.Collections.Generic;
using UnityEngine;

public static class SleepQuality
{
    private static int _sleepCurfewHour = 23;
    private static float _pastCurfewPenalty = 0.3f;
    public const float SLEEP_RECOVERY_RATIO = 0.5f;
    public const float DIET_RECOVERY_RATIO = 0.5f;
    
    private static Dictionary<Temperature, float> _sleepEnergyRecoveryRatios = new Dictionary<Temperature, float>
    {
        [Temperature.Freezing] = 0.3f,
        [Temperature.Cold] = 0.5f,
        [Temperature.Neutral] = 0.70f,
        [Temperature.Warm] = 1f,
        [Temperature.Hot] = 0.5f
    };

    private static Dictionary<Temperature, int> _awakeHours = new Dictionary<Temperature, int>
    {
        [Temperature.Freezing] = 5,
        [Temperature.Cold] = 6,
        [Temperature.Neutral] = 7,
        [Temperature.Warm] = 7,
        [Temperature.Hot] = 6
    };

    private static Dictionary<Temperature, string> _awakeMessages = new Dictionary<Temperature, string>
    {
        [Temperature.Freezing] = "the night was freezing. you slept poorly.",
        [Temperature.Cold] = "the night was cold, you slept okay.",
        [Temperature.Neutral] = "the night was comfortable. you slept well.",
        [Temperature.Warm] = "the night was warm. you slept great.",
        [Temperature.Hot] = "the night was hot. you slept okay."
    };

    public static int GetAwakeHour(Temperature playerTemperature)
    {
        if (_awakeHours.TryGetValue(playerTemperature, out var _hour))
            return _hour;

        Debug.LogError("There is no awake hour for the given temperature");
        return 7; // default to 7am
    }

    public static string GetAwakeMessage(Temperature playerTemperature)
    {
        if (_awakeMessages.TryGetValue(playerTemperature, out var _message))
            return _message;

        Debug.LogError("There is no awake hour for the given temperature");
        return "";
    }

    /// <returns> The players total energy after sleep </returns>
    public static float GetSleepRecoveryRatio(Temperature playerTemperature)
    {
        if (_sleepEnergyRecoveryRatios.TryGetValue(playerTemperature, out var _recoveryRatio))
            return _recoveryRatio - GetPastCurfewPenalty();

        Debug.LogError("There is no sleep recover ratio for the given temperature");
        return 0;
    }

    private static float GetPastCurfewPenalty()
    {
        return GameClock.Instance.GameHour.Value >= _sleepCurfewHour ? _pastCurfewPenalty : 0;
    }
}