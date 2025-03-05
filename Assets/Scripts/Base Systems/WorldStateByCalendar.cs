using System.Collections.Generic;
using ReactiveUnity;
using UnityEngine;

[CreateAssetMenu(fileName = "WorldState", menuName = "Base Systems/World State")]
public class WorldStateByCalendar : ScriptableObject
{
    public enum WaterStates { Puddles, Flood, Full, Shallow, PostFlood };
    public enum RainStates { HeavyRain, NoRain };
    private static Dictionary<(int gameYear, GameClock.Seasons season, int gameDay), WaterStates> _waterCalendarSpecific;
    private static Dictionary<(int gameYear, GameClock.Seasons season), WaterStates> _waterCalendarGeneral;
    public static Reactive<WaterStates> WaterState = new Reactive<WaterStates>(WaterStates.Shallow);
    public static Reactive<RainStates> RainState = new Reactive<RainStates>(RainStates.NoRain);
    public static Temperature OutsideTemperature = Temperature.Freezing;

    void OnEnable()
    {
        GameClock.Instance.OnGameDayTick += UpdateWorldState;
    }

    void OnDisable()
    {
        GameClock.Instance.OnGameDayTick -= UpdateWorldState;
    }

    private static void InitializeWaterState()
    {
        _waterCalendarSpecific = new()
        {
            [(1, GameClock.Seasons.Spring, 11)] = WaterStates.Puddles,
            [(1, GameClock.Seasons.Spring, 12)] = WaterStates.Shallow,
            [(1, GameClock.Seasons.Spring, 13)] = WaterStates.Full,
            [(1, GameClock.Seasons.Spring, 14)] = WaterStates.Flood,
            [(1, GameClock.Seasons.Spring, 15)] = WaterStates.Flood,
            [(1, GameClock.Seasons.Summer, 1)] = WaterStates.PostFlood,
            [(1, GameClock.Seasons.Summer, 2)] = WaterStates.PostFlood,
        };

        _waterCalendarGeneral = new()
        {
            [(1, GameClock.Seasons.Summer)] = WaterStates.Full,
            [(1, GameClock.Seasons.Fall)] = WaterStates.Shallow,
        };
    }

    private static void SetWaterState()
    {
        if (_waterCalendarSpecific.TryGetValue((GameClock.Instance.GameYear,
                                               GameClock.Instance.GameSeason,
                                               GameClock.Instance.GameDay),
                                               out WaterStates result))
        {
            WaterState.Value = result;
            return;
        }

        if (_waterCalendarGeneral.TryGetValue((GameClock.Instance.GameYear,
                                              GameClock.Instance.GameSeason),
                                              out WaterStates resultGeneral))
        {
            WaterState.Value = resultGeneral;
            return;
        }
        Debug.LogError("Waterstate defaulted, no calendar date match.");
        Debug.Log("Current Date: " + GameClock.Instance.GameYear + " " + GameClock.Instance.GameSeason + " " + GameClock.Instance.GameDay);
        Debug.Log("Current Time: " + GameClock.Instance.GameHour + ":" + GameClock.Instance.GameMinute);
        WaterState.Value = WaterStates.Shallow;
    }

    public static void UpdateWorldState()
    {
        Debug.Log("world state updated");
        if (_waterCalendarSpecific == null)
            InitializeWaterState();
        SetRainState();
        SetWaterState();
    }

    private static void SetRainState()
    {
        switch (GameClock.Instance.GameSeason)
        {
            case GameClock.Seasons.Spring:
                RainState.Value = RainStates.HeavyRain;
                break;
            case GameClock.Seasons.Summer:
                RainState.Value = RainStates.NoRain;
                break;
            default:
                RainState.Value = RainStates.NoRain;
                break;
        }
    }
}