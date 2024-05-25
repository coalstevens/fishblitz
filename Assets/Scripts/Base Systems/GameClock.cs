using System.Collections.Generic;
using UnityEngine;
using ReactiveUnity;
using UnityEditor.Build.Content;

public class GameClock : Singleton<GameClock>
{
    private float _timeBuffer = 0;
    private float _gameMinuteInRealSeconds; // Calculated in Start
    public enum Seasons {Spring, EndOfSpring, Summer, EndOfSummer, Fall, EndOfFall, Winter, EndOfWinter};
    public List<string> SeasonNames = new List<string> {"Spring", "EndOfSpring", "Summer", "EndOfSummer", "Fall", "EndOfFall", "Winter", "EndOfWinter"};
    [SerializeField] private float _gameDayInRealMinutes = 1f;
    [SerializeField] int _numRegularSeasonDays = 10;
    [SerializeField] int _numTransitionSeasonDays = 5; 
    public bool Paused = false;

    [Header("Game Start Date/Time")] 
    public Reactive<int> GameYear = new Reactive<int>(1);
    public Reactive<Seasons> GameSeason = new Reactive<Seasons>(Seasons.EndOfSpring);
    public Reactive<int> GameDay = new Reactive<int>(1);
    public Reactive<int> GameHour = new Reactive<int>(21);
    public Reactive<int> GameMinute = new Reactive<int>(0);
    
    void Start() {
        _gameMinuteInRealSeconds = _gameDayInRealMinutes * 60 / 1440;
        IncrementGameMinute(); // If the clock is paused this loads in some things to the correct time
    }
    
    void Update() {
        if (Paused)
            return;

        _timeBuffer += Time.deltaTime;

        if(_timeBuffer >= _gameMinuteInRealSeconds) {
            _timeBuffer -= _gameMinuteInRealSeconds;
            IncrementGameMinute();
        }  
    }
    public void PauseGameClock() {
        Paused = true;
    }
    public void ResumeGameClock() {
        Paused = false;
    }
    void IncrementGameMinute() {
            if (GameMinute.Value >= 59) {
                GameMinute.Value = 0;
                IncrementGameHour();
                return;
            }
            GameMinute.Value++;
        }

    void IncrementGameHour() {
        if (GameHour.Value >= 23) {
            GameHour.Value = 0;
            IncrementGameDay();
            return;
        }
        GameHour.Value++;
    }

    void IncrementGameDay() {
        if (GameDay.Value >= _numRegularSeasonDays + _numTransitionSeasonDays) {
            GameDay.Value = 1;
            IncrementSeason();
            return;
        }
        if (GameDay.Value == _numRegularSeasonDays + 1) {
            IncrementSeason();
        }
        GameDay.Value++;
    }

    void IncrementSeason() {
        if (GameSeason.Value == Seasons.EndOfWinter) {
            GameSeason.Value = Seasons.Spring; 
            GameYear.Value++;
        }
        GameSeason.Value++;
    }

    public void SetTime(int minute, int hour) {
        GameMinute.Value = minute;
        GameHour.Value = hour;
    }

    public void SetTime(int minute, int hour, int day) {
        GameMinute.Value = minute;
        GameHour.Value = hour;
        GameDay.Value = day;
    }

    public void SetTime(int minute, int hour, int day, Seasons season) {
        GameMinute.Value = minute;
        GameHour.Value = hour;
        GameDay.Value = day;
        GameSeason.Value = season;
    }

    public void SetTime(int minute, int hour, int day, Seasons season, int year) {
        GameMinute.Value = minute;
        GameHour.Value = hour;
        GameDay.Value = day;
        GameSeason.Value = season;
        GameYear.Value = year;
    }

    public static GameClockCapture GenerateCapture() {
        return new GameClockCapture(Instance);
    }
    public void SkipToTime(int hour, int minute) {
        while (GameHour.Value != hour || GameMinute.Value != minute)
            IncrementGameMinute();
    }

    public void SkipTime(int minutes) {
        for (int i = 0; i < minutes; i++) {
            IncrementGameMinute();
        }
    }

    public static int CalculateElapsedGameMinutesSinceTime(GameClockCapture pastTime) {
        GameClockCapture _currentTime = new GameClockCapture(Instance);
        int _elapsedGameMinutes = Instance.CalculateElapsedGameMinutesSinceZeroTime(_currentTime) - Instance.CalculateElapsedGameMinutesSinceZeroTime(pastTime);
        return (_elapsedGameMinutes > 0) ? _elapsedGameMinutes : 0;
    }

    private int CalculateElapsedGameMinutesSinceZeroTime(GameClockCapture _time) {
        int _elapsedGameMinutes = 0;
        _elapsedGameMinutes += _time.GameMinute;
        _elapsedGameMinutes += _time.GameHour * 60;
    
        for (int i = 0; i < (int) _time.GameSeason; i++) {
            if (i % 2 == 0) {
                //regular season
                _elapsedGameMinutes += Instance._numRegularSeasonDays * 24 * 60;
            }
            else {
                //transition season
                _elapsedGameMinutes += Instance._numTransitionSeasonDays * 24 * 60;
            }
        }

        _elapsedGameMinutes += _time.GameYear * 4 * (Instance._numRegularSeasonDays + Instance._numTransitionSeasonDays) * 24 * 60;
        
        return _elapsedGameMinutes;
    }
}
public class GameClockCapture {
        public int GameMinute;
        public int GameHour;
        public int GameDay;
        public GameClock.Seasons GameSeason;
        public int GameYear;

        /// <summary>
        /// Initalizes all values to 0 and Season to EndOfSpring;
        /// </summary>
        public GameClockCapture() {
            GameMinute = 0;
            GameHour = 0;
            GameDay = 0;
            GameSeason = GameClock.Seasons.EndOfSpring;
            GameYear =0;
        }

        /// <summary>
        /// Generates a capture of the current time
        /// </summary>
        public GameClockCapture(GameClock _gameClock) {
            GameMinute = _gameClock.GameMinute.Value;
            GameHour = _gameClock.GameHour.Value;
            GameDay = _gameClock.GameDay.Value;
            GameSeason = _gameClock.GameSeason.Value;
            GameYear = _gameClock.GameYear.Value;
        }
}
