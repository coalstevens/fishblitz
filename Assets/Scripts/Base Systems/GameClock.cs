using System.Collections.Generic;
using UnityEngine;
using ReactiveUnity;
using System;

public class GameClock : Singleton<GameClock>
{
    private float _timeBuffer = 0;
    private float _gameMinuteInRealSeconds; // Calculated in Start

    [Serializable]
    private class DayPeriodRange {
        public DayPeriods Period;
        public int StartHour;
        public int EndHour;
    }
    public enum Seasons { Spring, EndOfSpring, Summer, EndOfSummer, Fall, EndOfFall, Winter, EndOfWinter };
    public enum DayPeriods {SUNRISE, DAY, SUNSET, NIGHT};
    public List<string> SeasonNames = new List<string> { "Spring", "EndOfSpring", "Summer", "EndOfSummer", "Fall", "EndOfFall", "Winter", "EndOfWinter" };
    [SerializeField] private float _gameDayInRealMinutes = 1f;
    [SerializeField] private int _numRegularSeasonDays = 10;
    [SerializeField] private int _numTransitionSeasonDays = 5;
    [SerializeField] private List<DayPeriodRange> _dayPeriodRanges = new List<DayPeriodRange>();
    public bool GameclockPaused = false; // State of gameClock (Gameclock doesn't increment but gametime still passes)

    [Header("Game Start Date/Time")]
    public Reactive<int> GameYear = new Reactive<int>(1);
    public Reactive<Seasons> GameSeason = new Reactive<Seasons>(Seasons.EndOfSpring);
    public Reactive<int> GameDay = new Reactive<int>(11);
    public Reactive<int> GameHour = new Reactive<int>(21);
    public Reactive<int> GameMinute = new Reactive<int>(0);
    public Reactive<bool> GameIsPaused = new Reactive<bool>(false); // State of true gametime

    void Start()
    {
        _gameMinuteInRealSeconds = _gameDayInRealMinutes * 60 / 1440;
        IncrementGameMinute(); // If the clock is paused this loads in some things to the correct time
    }

    void Update()
    {
        if (GameclockPaused)
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
        GameclockPaused = true;
    }
    public void ResumeGameClock()
    {
        GameclockPaused = false;
    }
    void IncrementGameMinute()
    {
        if (GameMinute.Value >= 59)
        {
            GameMinute.Value = 0;
            IncrementGameHour();
            return;
        }
        GameMinute.Value++;
    }

    void IncrementGameHour()
    {
        if (GameHour.Value >= 23)
        {
            GameHour.Value = 0;
            IncrementGameDay();
            return;
        }
        GameHour.Value++;
    }

    void IncrementGameDay()
    {
        if (GameDay.Value == _numRegularSeasonDays + _numTransitionSeasonDays)
        {
            GameDay.Value = 1;
            IncrementSeason();
            return;
        }
        if (GameDay.Value == _numRegularSeasonDays)
        {
            IncrementSeason();
        }
        GameDay.Value++;
    }

    void IncrementSeason()
    {
        // end of year case
        if (GameSeason.Value == Seasons.EndOfWinter)
        {
            GameSeason.Value = Seasons.Spring;
            GameYear.Value++;
            return;
        }

        // normal case
        GameSeason.Value++;
    }

    public void SetTime(int minute, int hour)
    {
        GameMinute.Value = minute;
        GameHour.Value = hour;
    }

    public void SetTime(int minute, int hour, int day)
    {
        GameMinute.Value = minute;
        GameHour.Value = hour;
        GameDay.Value = day;
    }

    public void SetTime(int minute, int hour, int day, Seasons season)
    {
        GameMinute.Value = minute;
        GameHour.Value = hour;
        GameDay.Value = day;
        GameSeason.Value = season;
    }

    public void SetTime(int minute, int hour, int day, Seasons season, int year)
    {
        GameMinute.Value = minute;
        GameHour.Value = hour;
        GameDay.Value = day;
        GameSeason.Value = season;
        GameYear.Value = year;
    }

    public static GameClockCapture GenerateCapture()
    {
        return new GameClockCapture(Instance);
    }
    public void SkipToTime(int day, int hour, int minute)
    {
        if (day == _numRegularSeasonDays + _numTransitionSeasonDays + 1)
            day = 1;
            
        while (GameDay.Value != day || GameHour.Value != hour || GameMinute.Value != minute)
            IncrementGameMinute();
    }

    public void SkipTime(int minutes)
    {
        for (int i = 0; i < minutes; i++)
        {
            IncrementGameMinute();
        }
    }

    public static int CalculateElapsedGameMinutesSinceTime(GameClockCapture pastTime)
    {
        GameClockCapture _currentTime = new GameClockCapture(Instance);
        int _elapsedGameMinutes = Instance.CalculateElapsedGameMinutesSinceZeroTime(_currentTime) - Instance.CalculateElapsedGameMinutesSinceZeroTime(pastTime);
        return (_elapsedGameMinutes > 0) ? _elapsedGameMinutes : 0;
    }

    private int CalculateElapsedGameMinutesSinceZeroTime(GameClockCapture _time)
    {
        int _elapsedGameMinutes = 0;
        _elapsedGameMinutes += _time.GameMinute;
        _elapsedGameMinutes += _time.GameHour * 60;

        for (int i = 0; i < (int)_time.GameSeason; i++)
        {
            if (i % 2 == 0)
            {
                //regular season
                _elapsedGameMinutes += Instance._numRegularSeasonDays * 24 * 60;
            }
            else
            {
                //transition season
                _elapsedGameMinutes += Instance._numTransitionSeasonDays * 24 * 60;
            }
        }

        _elapsedGameMinutes += _time.GameYear * 4 * (Instance._numRegularSeasonDays + Instance._numTransitionSeasonDays) * 24 * 60;

        return _elapsedGameMinutes;
    }

    public void PauseGame()
    {
        // already paused
        if (GameIsPaused.Value)
            return;

        GameIsPaused.Value = true;
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        // already resumed
        if (!GameIsPaused.Value)
            return;

        GameIsPaused.Value = false;
        Time.timeScale = 1f;
    }

    public DayPeriods GetDayPeriod()
    {
        foreach (var _range in _dayPeriodRanges)
        {
            if (_range.StartHour <= _range.EndHour)
            {
                if (GameHour.Value >= _range.StartHour && GameHour.Value < _range.EndHour)
                    return _range.Period;
            }
            else
            {
                // Handle range that wraps around midnight (e.g., 22 to 2)
                if (GameHour.Value >= _range.StartHour || GameHour.Value < _range.EndHour)
                    return _range.Period;
            }
        }

        Debug.LogError("Day ranges should include all possible hours.");
        return DayPeriods.DAY; 
    }

    public Seasons GetGeneralSeason() {
        switch(GameSeason.Value) {
            case Seasons.Fall:
            case Seasons.Spring:
            case Seasons.Summer:
            case Seasons.Winter:
                return GameSeason.Value;
            case Seasons.EndOfFall:
                return Seasons.Fall;
            case Seasons.EndOfSpring:
                return Seasons.Spring;
            case Seasons.EndOfSummer:
                return Seasons.Summer;
            case Seasons.EndOfWinter:
                return Seasons.Winter; 
            default:
                Debug.LogError("Unexpected code path.");
                return GameSeason.Value;
        }
    }
}

public class GameClockCapture
{
    public int GameMinute;
    public int GameHour;
    public int GameDay;
    public GameClock.Seasons GameSeason;
    public int GameYear;

    /// <summary>
    /// Initalizes all values to 0 and Season to EndOfSpring;
    /// </summary>
    public GameClockCapture()
    {
        GameMinute = 0;
        GameHour = 0;
        GameDay = 0;
        GameSeason = GameClock.Seasons.EndOfSpring;
        GameYear = 0;
    }

    /// <summary>
    /// Generates a capture of the current time
    /// </summary>
    public GameClockCapture(GameClock _gameClock)
    {
        GameMinute = _gameClock.GameMinute.Value;
        GameHour = _gameClock.GameHour.Value;
        GameDay = _gameClock.GameDay.Value;
        GameSeason = _gameClock.GameSeason.Value;
        GameYear = _gameClock.GameYear.Value;
    }
}
