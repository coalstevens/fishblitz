using System.Collections.Generic;
using UnityEngine;

public enum RiverStates{Puddles, Flood, FullGrass, Shallow, FullDirt};                   
public class OutsideSceneStateCalendar : MonoBehaviour
    {
    [SerializeField] private GameObject _waterFull;
    [SerializeField] private GameObject _waterPuddles;
    [SerializeField] private GameObject _waterShallow;
    [SerializeField] private GameObject _waterFlood;
    [SerializeField] private GameObject _banksGrass;
    [SerializeField] private GameObject _banksDirt;
    [SerializeField] private Transform _shallowRocksSubmerged;
    [SerializeField] private Transform _allRocks;
    [SerializeField] private Rain _rainManager;

    private Dictionary<(int gameYear, GameClock.Seasons season, int gameDay), RiverStates> _riverCalendar;

    private void Start()
    {
        InitalizeCalendar();
        DisableAllRiverTilemaps();
        _riverCalendar.TryGetValue((GameClock.Instance.GameYear.Value,
                                   GameClock.Instance.GameSeason.Value,
                                   GameClock.Instance.GameDay.Value),
                                   out RiverStates result);
        SetSceneState(result);                           
    }

    private void InitalizeCalendar() {
        _riverCalendar = new()
        {
            [(1, GameClock.Seasons.EndOfSpring, 11)] = RiverStates.Puddles,
            [(1, GameClock.Seasons.EndOfSpring, 12)] = RiverStates.Shallow,
            [(1, GameClock.Seasons.EndOfSpring, 13)] = RiverStates.FullGrass,
            [(1, GameClock.Seasons.EndOfSpring, 14)] = RiverStates.Flood,
            [(1, GameClock.Seasons.EndOfSpring, 15)] = RiverStates.Flood,
            [(1, GameClock.Seasons.Summer, 1)] = RiverStates.FullDirt,
            [(1, GameClock.Seasons.Summer, 2)] = RiverStates.FullDirt,
            [(1, GameClock.Seasons.Summer, 3)] = RiverStates.FullGrass,
            [(1, GameClock.Seasons.Summer, 4)] = RiverStates.FullGrass
        };
    }

    private void SetSceneState(RiverStates newState) {
        HandleRiver(newState);
        HandleRain();
    }

    private void HandleRain() {
        switch(GameClock.Instance.GameSeason.Value) {
            case GameClock.Seasons.EndOfSpring:
                _rainManager.State.Value = Rain.States.HeavyRain;
                break;
            case GameClock.Seasons.Summer:
                _rainManager.State.Value = Rain.States.NoRain;
                break;
        }
    }

    private void HandleRiver(RiverStates newState) {
        DisableAllRiverTilemaps();
        switch (newState) {    
            case RiverStates.Puddles:
                _waterPuddles.SetActive(true);
                _banksGrass.SetActive(true);
                break;
            case RiverStates.Shallow:
                _waterShallow.SetActive(true);
                _banksGrass.SetActive(true);
                SubmergeShallowRocks();
                break;
            case RiverStates.Flood:
                _waterFlood.SetActive(true);
                _banksDirt.SetActive(true);
                SubmergeAllRocks();
                break;
            case RiverStates.FullDirt:
                _waterFull.SetActive(true);
                _banksDirt.SetActive(true);
                SubmergeAllRocks();
                break;
            case RiverStates.FullGrass:
                _banksGrass.SetActive(true);
                _waterFull.SetActive(true);
                SubmergeAllRocks();
                break;
            default:
                _banksGrass.SetActive(true);
                _waterFull.SetActive(true);
                break;
        }
    }
    private void SubmergeShallowRocks() {
        foreach (SpriteRenderer rock in _shallowRocksSubmerged.GetComponentsInChildren<SpriteRenderer>()) {
            rock.sortingLayerName = "Background";
            rock.sortingOrder = -6;
        }
    }
    private void SubmergeAllRocks() {
        SubmergeShallowRocks();
        foreach (SpriteRenderer rock in _allRocks.GetComponentsInChildren<SpriteRenderer>()) {
            rock.sortingLayerName = "Background";
            rock.sortingOrder = -6;
        }
    }

    private void DisableAllRiverTilemaps() {
        _waterFlood.SetActive(false);
        _waterFull.SetActive(false);
        _waterPuddles.SetActive(false);
        _waterShallow.SetActive(false);
        _banksGrass.SetActive(false);
        _banksDirt.SetActive(false);
    }
}
