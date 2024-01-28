using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using ReactiveUnity;
using System;
public class GameClock : MonoBehaviour
{
    private static GameClock _instance;
    public static GameClock Instance
    {
        get
        {
            // If the instance doesn't exist, find it in the scene
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameClock>();

                if (_instance == null)
                {
                    Debug.LogError("Gameclock object does not exist");
                }
            }

            return _instance;
        }
    }
    public enum Seasons {Spring, EndOfSpring, Summer, EndOfSummer, Fall, EndOfFall, Winter, EndOfWinter};
    public List<string> SeasonNames = new List<string> {"Spring", "EndOfSpring", "Summer", "EndOfSummer", "Fall", "EndOfFall", "Winter", "EndOfWinter"};
    [SerializeField] private float _gameDayInRealMinutes = 1f;
    private float _gameMinuteInRealSeconds; // Calculated in Start
    [SerializeField] int _numRegularSeasonDays = 10;
    [SerializeField] int _numTransitionSeasonDays = 5; 
    public bool Paused = false;

    [Header("Game Start Date/Time")] 
    public Reactive<int> GameYear = new Reactive<int>(1);
    public Reactive<Seasons> _gameSeason = new Reactive<Seasons>(Seasons.EndOfSpring);
    public Reactive<int> GameDay = new Reactive<int>(1);
    public Reactive<int> GameHour = new Reactive<int>(21);
    public Reactive<int> GameMinute = new Reactive<int>(0);
    private float _timeBuffer = 0;
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject); // Optional: Keeps the GameObject with the singleton alive between scenes
        }
        else
        {
            Destroy(gameObject); // Ensures that only one instance of the singleton exists
        }
    }

    void Start() {
        _gameMinuteInRealSeconds = _gameDayInRealMinutes * 60 / 1440;
        IncrementGameMinute(); // If the clock is paused this loads in some things to the correct time
    }
    
    void Update() {
        if (Paused) {
            return;
        }
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
        if (_gameSeason.Value == Seasons.EndOfWinter) {
            _gameSeason.Value = Seasons.Spring; 
            GameYear.Value++;
        }
        _gameSeason.Value++;
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
        _gameSeason.Value = season;
    }

    public void SetTime(int minute, int hour, int day, Seasons season, int year) {
        GameMinute.Value = minute;
        GameHour.Value = hour;
        GameDay.Value = day;
        _gameSeason.Value = season;
        GameYear.Value = year;
    }

    

}

