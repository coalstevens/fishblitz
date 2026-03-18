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
    [SerializeField] private WeightyObjectStackData _playerWheelBarrow;
    [SerializeField] private WeightyObjectStackData _carriedObjects;
    [Header("Move Speeds")]
    [SerializeField] private float _defaultMoveSpeed = 3.5f;
    [SerializeField] private float _wheelbarrowMoveSpeed = 3.5f;
    [Header("Base Accelerations")]
    [SerializeField] private float _baseAcceleration = 3f;
    [SerializeField] private float _baseDeceleration = 20f;
    [SerializeField] private float _baseWheelbarrowAcceleration = 10f;
    [SerializeField] private float _baseWheelbarrowDeceleration = 20f;
    [Header("Carrying")]
    [SerializeField] private float _carryingPerObjectAccelReduction = 1f;
    [SerializeField] private float _carryingPerObjectMoveSpeedReduction = 0.25f;
    [Header("Wheelbarrow")]
    [SerializeField] private float _wheelbarrowPerObjectAccelReduction = 1f;
    [SerializeField] private float _wheelbarrowPerObjectMoveSpeedReduction = 0.25f;
    [Header("Observation")]
    [SerializeField] private float _currentAcceleration;
    [SerializeField] private float _currentDeceleration;
    [SerializeField] private float _currentMaxMoveSpeed;
    public Vector2 CurrentMotion => _currentMotion;
    public Reactive<CompassDirection> Direction = new Reactive<CompassDirection>(CompassDirection.SouthEast);
    public Reactive<PlayerStates> PlayerState = new Reactive<PlayerStates>(PlayerStates.Idle);
    private Vector2 _currentMotion = Vector2.zero;
    private Vector2 _targetVelocity = Vector2.zero;
    private Vector2 _currentVelocity = Vector2.zero;
    private Rigidbody2D _rb;
    private CardinalVector _maxMoveSpeeds; // Upper limit of player velocity
    private CardinalVector _moveSpeedsMultiplier; // Can be publicly adjusted to impact player movespeed

    private void Awake()
    {
        _instance = this;
        _rb = GetComponent<Rigidbody2D>();
        transform.position = _playerData.SceneSpawnPosition;
        _maxMoveSpeeds = new CardinalVector(_defaultMoveSpeed);
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

        if (_currentMotion.x > 0 && _currentMotion.y > 0)
            Direction.Value = CompassDirection.NorthEast;
        else if (_currentMotion.x > 0 && _currentMotion.y < 0)
            Direction.Value = CompassDirection.SouthEast;
        else if (_currentMotion.x < 0 && _currentMotion.y > 0)
            Direction.Value = CompassDirection.NorthWest;
        else if (_currentMotion.x < 0 && _currentMotion.y < 0)
            Direction.Value = CompassDirection.SouthWest;
        else if (_currentMotion.x > 0)
            Direction.Value = CompassDirection.East;
        else if (_currentMotion.x < 0)
            Direction.Value = CompassDirection.West;
        else if (_currentMotion.y > 0)
            Direction.Value = CompassDirection.North;
        else if (_currentMotion.y < 0)
            Direction.Value = CompassDirection.South;

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
        if (PlayerState.Value != PlayerStates.Idle &&
            PlayerState.Value != PlayerStates.Running &&
            PlayerState.Value != PlayerStates.Birding &&
            PlayerState.Value != PlayerStates.BirdingRunning)
        {
            _rb.linearVelocity = Vector2.zero;
            _currentVelocity = Vector2.zero;
            _targetVelocity = Vector2.zero;
            return;
        }

        Vector2 normalizedInput = _currentMotion.magnitude > 0 ? _currentMotion.normalized : Vector2.zero;

        Vector2 scalarMoveSpeed;
        scalarMoveSpeed.x = _currentMotion.x >= 0 ? _maxMoveSpeeds.east * _moveSpeedsMultiplier.east :
                                                     _maxMoveSpeeds.west * _moveSpeedsMultiplier.west;
        scalarMoveSpeed.y = _currentMotion.y >= 0 ? _maxMoveSpeeds.north * _moveSpeedsMultiplier.north :
                                                     _maxMoveSpeeds.south * _moveSpeedsMultiplier.south;

        _targetVelocity = new Vector2(
            normalizedInput.x * scalarMoveSpeed.x,
            normalizedInput.y * scalarMoveSpeed.y
        );

        CalculateAcceleration();
        float currentRate = _currentMotion.magnitude > 0 ? _currentAcceleration : _currentDeceleration;
        _currentVelocity = Vector2.MoveTowards(_currentVelocity, _targetVelocity, currentRate * Time.fixedDeltaTime);

        _rb.linearVelocity = _currentVelocity;
    }

    // Things like wind will change the _moveSpeedsMultiplier
    public void SetMoveSpeedMultiplier(CardinalVector newMultiplier)
    {
        _moveSpeedsMultiplier = newMultiplier;
    }

    private void CalculateAcceleration()
    {
        if (_playerData.IsHoldingWheelBarrow.Value)
        {
            int wheelbarrowCount = _playerWheelBarrow != null ? _playerWheelBarrow.StoredObjects.Count : 0;
            _currentAcceleration = _baseWheelbarrowAcceleration - (wheelbarrowCount * _wheelbarrowPerObjectAccelReduction);
            _currentDeceleration = _baseWheelbarrowDeceleration;
            _currentMaxMoveSpeed = _wheelbarrowMoveSpeed - (wheelbarrowCount * _wheelbarrowPerObjectMoveSpeedReduction);
            _maxMoveSpeeds = new CardinalVector(_currentMaxMoveSpeed);
        }
        else if (_playerData.IsCarrying.Value)
        {
            int carriedCount = _carriedObjects != null ? _carriedObjects.StoredObjects.Count : 0;
            _currentAcceleration = _baseAcceleration - (carriedCount * _carryingPerObjectAccelReduction);
            _currentDeceleration = _baseDeceleration;
            _currentMaxMoveSpeed = _defaultMoveSpeed - (carriedCount * _carryingPerObjectMoveSpeedReduction);
            _maxMoveSpeeds = new CardinalVector(_currentMaxMoveSpeed);
        }
        else
        {
            _currentAcceleration = _baseAcceleration;
            _currentDeceleration = _baseDeceleration;
            _currentMaxMoveSpeed = _defaultMoveSpeed;
            _maxMoveSpeeds = new CardinalVector(_defaultMoveSpeed);
        }
    }
}
