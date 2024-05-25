using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using ReactiveUnity;
using UnityEngine.SceneManagement;
//using System.Numerics;
public enum FacingDirections
{
    North,
    South,
    West,
    East,
}

public enum PlayerStates
{
    Walking,
    Idle,
    Fishing,
    Axing,
    Catching,
    Celebrating,
}
public struct CardinalVector
{
    public float north;
    public float east;
    public float south;
    public float west;
    public CardinalVector(float defaultValue)
    {
        north = defaultValue;
        east = defaultValue;
        south = defaultValue;
        west = defaultValue;
    }
}
public class PlayerMovementController : MonoBehaviour
{
    private const float DEFAULT_MOVE_SPEED = 3.5f;
    private Vector2 _currentMotion = Vector2.zero;
    private Rigidbody2D _rb;
    public Reactive<FacingDirections> FacingDirection = new Reactive<FacingDirections>(FacingDirections.North);
    public Reactive<PlayerStates> PlayerState = new Reactive<PlayerStates>(global::PlayerStates.Idle);
    private CardinalVector _maxMoveSpeeds; // Upper limit of player velocity
    private CardinalVector _moveSpeedsMultiplier; // Can be publicly adjusted to impact player movespeed

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        transform.position = PlayerData.Instance.SceneSpawnPosition;
        _maxMoveSpeeds = new CardinalVector(DEFAULT_MOVE_SPEED);
        _moveSpeedsMultiplier = new CardinalVector(1);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        transform.position = PlayerData.Instance.SceneSpawnPosition;
    }

    public void OnMove(InputValue value)
    {
        _currentMotion = value.Get<Vector2>();
    }

    private void Update()
    {
        // Can only change direction when in Idle or Walking
        if (PlayerState.Value != PlayerStates.Idle && PlayerState.Value != PlayerStates.Walking)
            return;

        if (_currentMotion.x > 0)
            FacingDirection.Value = FacingDirections.East;
        else if (_currentMotion.x < 0)
            FacingDirection.Value = FacingDirections.West;
        else if (_currentMotion.y > 0)
            FacingDirection.Value = FacingDirections.North;
        else if (_currentMotion.y < 0)
            FacingDirection.Value = FacingDirections.South;

        if (_currentMotion.magnitude > 0)
            PlayerState.Value = PlayerStates.Walking;
        else
            PlayerState.Value = PlayerStates.Idle;
    }
    private void FixedUpdate()
    {
        // Can only move when in Idle or Walking
        if (PlayerState.Value != PlayerStates.Idle && PlayerState.Value != PlayerStates.Walking)
            return;

        Vector2 _scalarMoveSpeed;
        _scalarMoveSpeed.x = _currentMotion.x >= 0 ? _maxMoveSpeeds.east * _moveSpeedsMultiplier.east :
                                                        _maxMoveSpeeds.west * _moveSpeedsMultiplier.west;
        _scalarMoveSpeed.y = _currentMotion.y >= 0 ? _maxMoveSpeeds.north * _moveSpeedsMultiplier.north :
                                                        _maxMoveSpeeds.south * _moveSpeedsMultiplier.south;
        Vector2 _newPos = _rb.position + (_currentMotion * Time.fixedDeltaTime * _scalarMoveSpeed);
        _rb.MovePosition(_newPos);
    }

    // Things like wind will change the _moveSpeedsMultiplier
    public void SetMoveSpeedMultiplier(CardinalVector newMultiplier)
    {
        _moveSpeedsMultiplier = newMultiplier;
    }
}
