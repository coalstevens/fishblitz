using System.Collections.Generic;
using UnityEngine;

public enum RiverStates{Puddles, Flood, FullGrass, Shallow, FullDirt};                   
public class OutsideSceneStateCalendar : MonoBehaviour
    {
    [SerializeField] GameObject _waterFull;
    [SerializeField] GameObject _waterPuddles;
    [SerializeField] GameObject _waterShallow;
    [SerializeField] GameObject _waterFlood;
    [SerializeField] GameObject _banksGrass;
    [SerializeField] GameObject _banksDirt;

    Dictionary<(int gameYear, GameClock.Seasons season, int gameDay), RiverStates> _riverCalendar;

    void Start()
    {
        InitalizeCalendar();
        DisableAllVariableGameObjects();
        SetSceneState(_riverCalendar[(GameClock.Instance.GameYear.Value,
                                      GameClock.Instance.GameSeason.Value,
                                      GameClock.Instance.GameDay.Value)]);
                                
    }

    void InitalizeCalendar() {
        _riverCalendar = new();
        _riverCalendar[(1, GameClock.Seasons.EndOfSpring, 1)] = RiverStates.Puddles; 
        _riverCalendar[(1, GameClock.Seasons.EndOfSpring, 2)] = RiverStates.Shallow; 
        _riverCalendar[(1, GameClock.Seasons.EndOfSpring, 3)] = RiverStates.FullGrass; 
        _riverCalendar[(1, GameClock.Seasons.EndOfSpring, 4)] = RiverStates.Flood;
        _riverCalendar[(1, GameClock.Seasons.EndOfSpring, 5)] = RiverStates.Flood;
        _riverCalendar[(1, GameClock.Seasons.Spring, 1)] = RiverStates.FullDirt;
        _riverCalendar[(1, GameClock.Seasons.Spring, 2)] = RiverStates.FullDirt;
    }
    void SetSceneState(RiverStates newState) {
        switch (newState) {    
            case RiverStates.Puddles:
                _waterPuddles.SetActive(true);
                _banksGrass.SetActive(true);
                break;
            case RiverStates.Shallow:
                _waterShallow.SetActive(true);
                _banksGrass.SetActive(true);
                break;
            case RiverStates.Flood:
                _waterFlood.SetActive(true);
                _banksDirt.SetActive(true);
                break;
            case RiverStates.FullDirt:
                _waterFull.SetActive(true);
                _banksDirt.SetActive(true);
                break;
            case RiverStates.FullGrass:
                _banksGrass.SetActive(true);
                _waterFull.SetActive(true);
                break;
            default:
                _banksGrass.SetActive(true);
                _waterFull.SetActive(true);
                break;
        }
    }

    void DisableAllVariableGameObjects() {
        _waterFlood.SetActive(false);
        _waterFull.SetActive(false);
        _waterPuddles.SetActive(false);
        _waterShallow.SetActive(false);
        _banksGrass.SetActive(false);
        _banksDirt.SetActive(false);
    }
}
