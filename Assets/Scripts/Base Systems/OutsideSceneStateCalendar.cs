using System.Collections.Generic;
using ReactiveUnity;
using UnityEngine;

[CreateAssetMenu(fileName = "WorldState", menuName = "Base Systems/World State")]
public class WorldStateByCalendar : ScriptableObject
{
    public enum WaterStates { Puddles, Flood, Full, Shallow, PostFlood };
    public enum RainStates { HeavyRain, NoRain };
    [SerializeField] private Rain _rainManager;
    private static Dictionary<(int gameYear, GameClock.Seasons season, int gameDay), WaterStates> _waterCalendarSpecific;
    private static Dictionary<(int gameYear, GameClock.Seasons season), WaterStates> _waterCalendarGeneral;
    public static Reactive<WaterStates> WaterState = new Reactive<WaterStates>(WaterStates.Shallow);
    public static Reactive<RainStates> RainState = new Reactive<RainStates>(RainStates.NoRain);

    private void Start()
    {
        UpdateWorldState();
    }

    private static void InitializeCalendar()
    {
        _waterCalendarSpecific = new()
        {
            [(1, GameClock.Seasons.EndOfSpring, 11)] = WaterStates.Puddles,
            [(1, GameClock.Seasons.EndOfSpring, 12)] = WaterStates.Shallow,
            [(1, GameClock.Seasons.EndOfSpring, 13)] = WaterStates.Full,
            [(1, GameClock.Seasons.EndOfSpring, 14)] = WaterStates.Flood,
            [(1, GameClock.Seasons.EndOfSpring, 15)] = WaterStates.Flood,
            [(1, GameClock.Seasons.Summer, 1)] = WaterStates.PostFlood,
            [(1, GameClock.Seasons.Summer, 2)] = WaterStates.PostFlood,
        };

        _waterCalendarGeneral = new()
        {
            [(1, GameClock.Seasons.Summer)] = WaterStates.Full,
            [(1, GameClock.Seasons.EndOfSummer)] = WaterStates.Full,
            [(1, GameClock.Seasons.Fall)] = WaterStates.Shallow,
        };
    }

    private static void SetWaterState()
    {
        
        if (_waterCalendarSpecific.TryGetValue((GameClock.Instance.GameYear.Value,
                                               GameClock.Instance.GameSeason.Value,
                                               GameClock.Instance.GameDay.Value),
                                               out WaterStates result))
        {
            WaterState.Value = result;
            return;
        }

        if (_waterCalendarGeneral.TryGetValue((GameClock.Instance.GameYear.Value,
                                              GameClock.Instance.GameSeason.Value),
                                              out WaterStates resultGeneral))
        {
            WaterState.Value = resultGeneral;
            return;
        }
        Debug.LogError("Waterstate defaulted, no calendar date match.");
        WaterState.Value = WaterStates.Shallow;
    }

    public static void UpdateWorldState()
    {
        if(_waterCalendarSpecific == null)  
            InitializeCalendar();
        SetRainState();
        SetWaterState();
    }

    private static void SetRainState()
    {
        switch (GameClock.Instance.GameSeason.Value)
        {
            case GameClock.Seasons.EndOfSpring:
                RainState.Value = RainStates.HeavyRain;
                break;
            case GameClock.Seasons.Summer:
                RainState.Value = RainStates.NoRain;
                break;
        }
    }
}