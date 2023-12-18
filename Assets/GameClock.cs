using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using ReactiveUnity;
using System;
public class GameClock : MonoBehaviour
{
    public enum Seasons {Spring, EndOfSpring, Summer, EndOfSummer, Fall, EndOfFall, Winter, EndOfWinter};
    public List<string> SeasonNames = new List<string> {"Spring", "EndOfSpring", "Summer", "EndOfSummer", "Fall", "EndOfFall", "Winter", "EndOfWinter"};
    [SerializeField] private float _gameDayInRealMinutes = 1f;
    private float _gameMinuteInRealSeconds; // Calculated in Start
    [SerializeField] int _numRegularSeasonDays = 10;
    [SerializeField] int _numTransitionSeasonDays = 5; 
    public bool _paused = false;

    [Header("Game Start Date/Time")] 
    public Reactive<int> _gameYear = new Reactive<int>(1);
    public Reactive<Seasons> _gameSeason = new Reactive<Seasons>(Seasons.EndOfSpring);
    public Reactive<int> _gameDay = new Reactive<int>(1);
    public Reactive<int> _gameHour = new Reactive<int>(21);
    public Reactive<int> _gameMinute = new Reactive<int>(0);
    private float _timeBuffer = 0;
    

    void Start() {
        _gameMinuteInRealSeconds = _gameDayInRealMinutes * 60 / 1440;
        IncrementGameMinute(); // If the clock is paused this loads in some things to the correct time
    }
    
    void Update() {
        if (_paused) {
            return;
        }
        _timeBuffer += Time.deltaTime;
        if(_timeBuffer >= _gameMinuteInRealSeconds) {
            _timeBuffer -= _gameMinuteInRealSeconds;
            IncrementGameMinute();
        }  
    }
    public void PauseGameClock() {
        _paused = true;
    }
    public void ResumeGameClock() {
        _paused = false;
    }
    void IncrementGameMinute() {
            if (_gameMinute.Value >= 59) {
                _gameMinute.Value = 0;
                IncrementGameHour();
                return;
            }
            _gameMinute.Value++;
        }

    void IncrementGameHour() {
        if (_gameHour.Value >= 23) {
            _gameHour.Value = 0;
            IncrementGameDay();
            return;
        }
        _gameHour.Value++;
    }

    void IncrementGameDay() {
        if (_gameDay.Value >= _numRegularSeasonDays + _numTransitionSeasonDays) {
            _gameDay.Value = 1;
            IncrementSeason();
            return;
        }
        if (_gameDay.Value == _numRegularSeasonDays + 1) {
            IncrementSeason();
        }
        _gameDay.Value++;
    }

    void IncrementSeason() {
        if (_gameSeason.Value == Seasons.EndOfWinter) {
            _gameSeason.Value = Seasons.Spring; 
            _gameYear.Value++;
        }
        _gameSeason.Value++;
    }

    public void SetTime(int minute, int hour) {
        _gameMinute.Value = minute;
        _gameHour.Value = hour;
    }

    public void SetTime(int minute, int hour, int day) {
        _gameMinute.Value = minute;
        _gameHour.Value = hour;
        _gameDay.Value = day;
    }

    public void SetTime(int minute, int hour, int day, Seasons season) {
        _gameMinute.Value = minute;
        _gameHour.Value = hour;
        _gameDay.Value = day;
        _gameSeason.Value = season;
    }

    public void SetTime(int minute, int hour, int day, Seasons season, int year) {
        _gameMinute.Value = minute;
        _gameHour.Value = hour;
        _gameDay.Value = day;
        _gameSeason.Value = season;
        _gameYear.Value = year;
    }

    

}

