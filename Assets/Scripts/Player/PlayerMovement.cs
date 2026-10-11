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

[RequireComponent(typeof(Rigidbody2D), typeof(PlayerSceneData))]
[RequireComponent(typeof(PlayerWheelBarrow), typeof(PlayerCarry))]
[RequireComponent(typeof(PlayerStateController))]
public class PlayerMovement : MonoBehaviour
{
    private static PlayerMovement _instance;
    public static PlayerMovement Instance
    {
        get
        {
            if (_instance == null)
                Debug.LogError("This object does not exist");
            return _instance;
        }
    }
    private PlayerSceneData _sceneData;
    private PlayerWheelBarrow _wheelBarrow;
    private PlayerCarry _playerCarry;
    [SerializeField] private PlayerMovementData _movementData;
    [Header("Observation")]
    [SerializeField] private float _currentAcceleration;
    [SerializeField] private float _currentDeceleration;
    [SerializeField] private float _currentMaxMoveSpeed;
    public Vector2 CurrentMotion => _currentMotion;
    public Reactive<CompassDirection> Direction = new Reactive<CompassDirection>(CompassDirection.SouthEast);
    private PlayerStateController _stateController;
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
        _sceneData = GetComponent<PlayerSceneData>();
        _wheelBarrow = GetComponent<PlayerWheelBarrow>();
        _playerCarry = GetComponent<PlayerCarry>();
        _stateController = GetComponent<PlayerStateController>();
        _maxMoveSpeeds = new CardinalVector(_movementData.DefaultMoveSpeed);
        _moveSpeedsMultiplier = new CardinalVector(1);
    }

    private void Start()
    {
        if (PlayerSceneData.HasPendingSpawn)
        {
            transform.position = PlayerSceneData.PendingSpawnPosition;
            _sceneData.SceneSpawnPosition = PlayerSceneData.PendingSpawnPosition;
            PlayerSceneData.HasPendingSpawn = false;
        }
    }

    public void OnMove(InputValue value)
    {
        _currentMotion = value.Get<Vector2>();
    }

    private void Update()
    {
        // Can only change direction and run while in a movement-allowed state
        if (!_stateController.PlayerState.Value.CanMove())
        {
            _stateController.SetRunning(false);
            return;
        }

        _stateController.SetRunning(_currentMotion.magnitude > 0);

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

        if (_currentMotion.magnitude == 0 && _stateController.PlayerState.Value == PlayerStates.BowCharging)
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Direction.Value = mouseWorld.x > transform.position.x
                ? CompassDirection.SouthEast
                : CompassDirection.SouthWest;
        }
    }

    private void FixedUpdate()
    {
        // Can only move when in a movement-allowed state
        if (!_stateController.PlayerState.Value.CanMove())
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
        if (_wheelBarrow.IsHoldingWheelBarrow.Value)
        {
            int wheelbarrowCount = _wheelBarrow.WheelBarrowStack.StoredObjects.Count;
            _currentAcceleration = _movementData.BaseWheelbarrowAcceleration - (wheelbarrowCount * _movementData.WheelbarrowPerObjectAccelReduction);
            _currentDeceleration = _movementData.BaseWheelbarrowDeceleration;
            _currentMaxMoveSpeed = _movementData.WheelbarrowMoveSpeed - (wheelbarrowCount * _movementData.WheelbarrowPerObjectMoveSpeedReduction);
            _maxMoveSpeeds = new CardinalVector(_currentMaxMoveSpeed);
        }
        else if (_playerCarry.IsCarrying.Value)
        {
            int carriedCount = _playerCarry.CarriedStack.StoredObjects.Count;
            _currentAcceleration = _movementData.BaseAcceleration - (carriedCount * _movementData.CarryingPerObjectAccelReduction);
            _currentDeceleration = _movementData.BaseDeceleration;
            _currentMaxMoveSpeed = _movementData.DefaultMoveSpeed - (carriedCount * _movementData.CarryingPerObjectMoveSpeedReduction);
            _maxMoveSpeeds = new CardinalVector(_currentMaxMoveSpeed);
        }
        else
        {
            _currentAcceleration = _movementData.BaseAcceleration;
            _currentDeceleration = _movementData.BaseDeceleration;
            _currentMaxMoveSpeed = _movementData.DefaultMoveSpeed;
            _maxMoveSpeeds = new CardinalVector(_movementData.DefaultMoveSpeed);
        }
    }
}
