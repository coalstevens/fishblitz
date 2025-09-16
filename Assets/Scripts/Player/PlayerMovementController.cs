using UnityEngine;
using UnityEngine.InputSystem;
using ReactiveUnity;
using NUnit.Framework;
using UnityEngine.Playables;
public enum CompassDirection
{
    North,
    South,
    West,
    East,
    NorthEast,
    NorthWest,
    SouthEast,
    SouthWest,
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
    public enum PlayerStates
    {
        Running,
        Idle,
        Fishing,
        Axing,
        Catching,
        Celebrating,
        Birding,
        BirdingRunning,
        PickingUp,
        Crouched
    }

    private static PlayerMovementController _instance;
    public static PlayerMovementController Instance
    {
        get
        {
            if (_instance == null)
                Debug.LogError("This object does not exist");
            return _instance;
        }
    }
    [SerializeField] private PlayerData _playerData;
    private const float DEFAULT_MOVE_SPEED = 3.5f;
    private Vector2 _currentMotion = Vector2.zero;
    public Vector2 CurrentMotion => _currentMotion;
    private Rigidbody2D _rb;
    public Reactive<CompassDirection> FacingDirection = new Reactive<CompassDirection>(CompassDirection.SouthEast);
    public Reactive<PlayerStates> PlayerState = new Reactive<PlayerStates>(PlayerStates.Idle);
    private CardinalVector _maxMoveSpeeds; // Upper limit of player velocity
    private CardinalVector _moveSpeedsMultiplier; // Can be publicly adjusted to impact player movespeed

    private void Awake()
    {
        _instance = this;
        _rb = GetComponent<Rigidbody2D>();
        transform.position = _playerData.SceneSpawnPosition;
        _maxMoveSpeeds = new CardinalVector(DEFAULT_MOVE_SPEED);
        _moveSpeedsMultiplier = new CardinalVector(1);

        Assert.IsNotNull(_rb);
    }

    public void OnMove(InputValue value)
    {
        _currentMotion = value.Get<Vector2>();
    }

    private void Update()
    {
        // Can only change direction 
        if (PlayerState.Value != PlayerStates.Idle &&
            PlayerState.Value != PlayerStates.Running &&
            PlayerState.Value != PlayerStates.Birding &&
            PlayerState.Value != PlayerStates.BirdingRunning)
            return;

        if (_currentMotion.x > 0)
            FacingDirection.Value = CompassDirection.SouthEast;
        else if (_currentMotion.x < 0)
            FacingDirection.Value = CompassDirection.SouthWest;

        if (_currentMotion.magnitude > 0)
        {
            if (PlayerState.Value == PlayerStates.Idle)
                PlayerState.Value = PlayerStates.Running;
            if (PlayerState.Value == PlayerStates.Birding)
                PlayerState.Value = PlayerStates.BirdingRunning;
        }
        else
        {
            if (PlayerState.Value == PlayerStates.Running)
                PlayerState.Value = PlayerStates.Idle;
            if (PlayerState.Value == PlayerStates.BirdingRunning)
                PlayerState.Value = PlayerStates.Birding;
        }
    }

    private void FixedUpdate()
    {
        // Can only move when in Idle or Walking
        if (PlayerState.Value != PlayerStates.Idle && PlayerState.Value != PlayerStates.Running)
        {
            _rb.linearVelocity = Vector2.zero;
            return;
        }

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
