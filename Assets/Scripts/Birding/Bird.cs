using System.Collections.Generic;
using UnityEngine;

// TODO add shinys to game?
public class Bird : MonoBehaviour
{
    [SerializeField] public string BirdName = "Chickadee";
    [SerializeField] public Sprite Icon;
    [SerializeField] public List<GameClock.Seasons> SpawnableSeasons = new();
    [SerializeField] public List<GameClock.DayPeriods> SpawnablePeriods = new();
    [Header("Instance Specific")]
    [SerializeField] public GameClock.DayPeriods PeriodSpawned;
    [SerializeField] public GameClock.Seasons SeasonSpawned;
    [SerializeField] public bool Caught = false;

    Rigidbody2D _rb;

    private void Awake() {
        _rb = GetComponent<Rigidbody2D>();
    }

    public Vector2 GetVelocity() {
        return _rb.velocity;
    }
}

