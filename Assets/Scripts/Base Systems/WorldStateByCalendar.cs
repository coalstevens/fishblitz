using System;
using System.Collections.Generic;
using UnityEngine;

public class WorldStateCalendar : Singleton<WorldStateCalendar>
{
    [Serializable]
    public class DayState
    {
        public int GameYear;
        public int GameDay;
        public GameClock.Seasons Season;
        public WorldState.WaterStates WaterState;
        public WorldState.RainStates RainState;
    }
    [SerializeField] private GameClock _gameclock;
    [SerializeField] private Logger _logger = new();
    [SerializeField] private List<DayState> _calendar = new();

    void OnEnable()
    {
        _gameclock.OnGameDayTick += UpdateWorldState;
    }

    void OnDisable()
    {
        _gameclock.OnGameDayTick -= UpdateWorldState;
    }

    public void UpdateWorldState()
    {
        DayState _currentDayState = _calendar.Find(dayState =>
            dayState.GameYear == GameClock.Instance.GameYear &&
            dayState.GameDay == GameClock.Instance.GameDay &&
            dayState.Season == GameClock.Instance.GameSeason);

        if (_currentDayState != null)
        {
            WorldState.WaterState.Value = _currentDayState.WaterState;
            WorldState.RainState.Value = _currentDayState.RainState;
            _logger.Info($"RainState: {WorldState.RainState.Value.ToString()}, WaterState: {WorldState.WaterState.Value.ToString()}");
            return;
        }

        Debug.LogError($"World state defaulted, no calendar date match. Current Date: {GameClock.Instance.GameYear} {GameClock.Instance.GameSeason} {GameClock.Instance.GameDay}, Current Time: {GameClock.Instance.GameHour}:{GameClock.Instance.GameMinute}");
        WorldState.WaterState.Value = WorldState.WaterStates.Shallow;
    }
}