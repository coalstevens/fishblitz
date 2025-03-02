using System;
using System.Collections.Generic;
using UnityEngine;
using ReactiveUnity;

public class GameClock : Singleton<GameClock>
{
    public enum Seasons { Spring, Summer, Fall, Winter }
    public enum DayPeriods { Sunrise, Day, Sunset, Night }
    public interface ITickable
    {
        void OnGameMinuteTick();
    }

    [Serializable]
    private class DayPeriodRange
    {
        public DayPeriods Period;
        public int StartHour;
        public int EndHour;
    }

    [NonSerialized] public List<string> SeasonNames = new List<string> { "Spring", "Summer", "Fall", "Winter" };
    [SerializeField] private float _gameDayInRealMinutes = 1f;
    [SerializeField] private int _numSeasonDays = 15;
    [SerializeField] private List<DayPeriodRange> _dayPeriodRanges = new List<DayPeriodRange>();
    [SerializeField] private Logger _logger = new();
    public bool GameClockPaused = false;

    [Header("Game Start Date/Time")]
    [SerializeField] private int _startDay = 1;
    [SerializeField] private int _startHour = 6;
    [SerializeField] private int _startMinute = 0;
    [SerializeField] private Seasons _startSeason = Seasons.Spring;
    [SerializeField] private int _startYear = 1;

    private static int _gameMinutesElapsed = 0;
    public int GameMinutesElapsed => _gameMinutesElapsed;
    public int GameMinute => _gameMinutesElapsed % 60;
    public int GameHour => (_gameMinutesElapsed / 60) % 24;
    public int GameDay => (_gameMinutesElapsed / (60 * 24)) % _numSeasonDays + 1;
    public Seasons GameSeason => (Seasons)((_gameMinutesElapsed / (60 * 24 * _numSeasonDays)) % 4);
    public int GameYear => _gameMinutesElapsed / (60 * 24 * _numSeasonDays * 4);
    public Reactive<bool> GameIsPaused = new Reactive<bool>(false);
    private float _timeBuffer = 0;
    private float _gameMinuteInRealSeconds;

    public event Action OnGameMinuteTick;
    public event Action OnGameHourTick;
    public event Action OnGameDayTick;
    public event Action OnGameSeasonTick;
    public event Action OnGameYearTick;

    private void OnEnable()
    {
        _gameMinuteInRealSeconds = _gameDayInRealMinutes * 60 / 1440;
        InitializeClock();
    }

    private void Update()
    {
        if (GameClockPaused)
            return;

        _timeBuffer += Time.deltaTime;

        if (_timeBuffer >= _gameMinuteInRealSeconds)
        {
            _timeBuffer -= _gameMinuteInRealSeconds;
            IncrementGameMinute();
        }
    }

    public void PauseGameClock()
    {
        GameClockPaused = true;
    }

    public void ResumeGameClock()
    {
        GameClockPaused = false;
    }

    private void IncrementGameMinute()
    {
        _gameMinutesElapsed++;

        OnGameMinuteTick?.Invoke();
        if (GameMinute == 0)
        {
            OnGameHourTick?.Invoke();
            if (GameHour == 0)
            {
                OnGameDayTick?.Invoke();
                if (GameDay == 0)
                {
                    OnGameSeasonTick?.Invoke();
                    if (GameSeason == Seasons.Spring)
                        OnGameYearTick?.Invoke();
                }
            }
        }
    }

    private void InitializeClock()
    {
        _gameMinutesElapsed = _startMinute + _startHour * 60 + (_startDay - 1) * 24 * 60 + (int)_startSeason * _numSeasonDays * 24 * 60 + _startYear * 4 * _numSeasonDays * 24 * 60;
        _logger.Info($"The time is {SeasonNames[(int)GameSeason]} {GameDay} {GameHour}:{GameMinute}");
    }

    public void SkipToTime(int day, int hour, int minute)
    {
        int targetMinutesElapsed = minute + hour * 60 + (day - 1) * 24 * 60 + (int)GameSeason * _numSeasonDays * 24 * 60 + GameYear * 4 * _numSeasonDays * 24 * 60;
        int minutesToIncrement = targetMinutesElapsed - _gameMinutesElapsed;

        for (int i = 0; i < minutesToIncrement; i++)
            IncrementGameMinute();

        _logger.Info($"The time is {SeasonNames[(int)GameSeason]} {GameDay} {GameHour}:{GameMinute}");
    }

    public void SkipTime(int minutes)
    {
        for (int i = 0; i < minutes; i++)
            IncrementGameMinute();
    }

    public static int CalculateElapsedGameMinutesSinceTime(int pastTime)
    {
        return _gameMinutesElapsed - pastTime;
    }

    public void PauseGame()
    {
        if (GameIsPaused.Value)
            return;

        GameIsPaused.Value = true;
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        if (!GameIsPaused.Value)
            return;

        GameIsPaused.Value = false;
        Time.timeScale = 1f;
    }

    public DayPeriods GetDayPeriod()
    {
        foreach (var range in _dayPeriodRanges)
        {
            if (range.StartHour <= range.EndHour)
            {
                if (GameHour >= range.StartHour && GameHour < range.EndHour)
                    return range.Period;
            }
            else
            {
                if (GameHour >= range.StartHour || GameHour < range.EndHour)
                    return range.Period;
            }
        }

        Debug.LogError("Day ranges should include all possible hours.");
        return DayPeriods.Day;
    }
}
