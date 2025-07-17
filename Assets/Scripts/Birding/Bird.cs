using System.Collections.Generic;
using ReactiveUnity;
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
    public Reactive<bool> Caught = new Reactive<bool>(false);

    [Header("Birding Game")]
    [SerializeField] public float TimetoCatchSeconds = 2f;
    [SerializeField] private AudioClip _captureSound;
    [SerializeField] private float _soundVolume;
    [SerializeField] private PlayerData _playerData;
    public Reactive<float> BeamHoveredElapsedSeconds = new(0f);
    private Collider2D _beam;
    Rigidbody2D _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        // _beam = BirdingGame.Instance.BeamCollider;
    }

    public Vector2 GetVelocity()
    {
        return _rb.linearVelocity;
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other != _beam) return;
        BeamHoveredElapsedSeconds.Value += Time.deltaTime;
        if (BeamHoveredElapsedSeconds.Value >= TimetoCatchSeconds && !Caught.Value)
            OnCaught();
    }

    private void OnCaught()
    {
        Caught.Value = true;
        AudioManager.Instance.PlaySFX(_captureSound, _soundVolume);
        _playerData.BirdingLog.AddToLog(BirdName, SeasonSpawned, PeriodSpawned);
    }
}

