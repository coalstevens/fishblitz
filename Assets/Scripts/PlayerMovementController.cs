using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using ReactiveUnity;
//using System.Numerics;
public enum Direction
{
    Up,
    Down,
    Left,
    Right,
}

public enum State
{
    Walking,
    Idle,
    Fishing,
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
    private Vector2 _currMotionVector = Vector2.zero;
    private Rigidbody2D _rb;

    //TODO change _fishing reactive to "Acting"? cursor should disappear in an action state/animation like fishing
    // _fishing currently just toggles the cursor display
    public Reactive<bool> Fishing = new Reactive<bool>(false);
    public Reactive<Direction> FacingDir = new Reactive<Direction>(Direction.Up);
    public Reactive<State> CurrState = new Reactive<State>(State.Idle);
    private CardinalVector _moveSpeed;
    private CardinalVector _moveSpeedMultiplier;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        transform.position = PlayerData.Instance.SceneSpawnPosition;
        _moveSpeed = new CardinalVector(DEFAULT_MOVE_SPEED);
        _moveSpeedMultiplier = new CardinalVector(1);
    }

    public void OnMove(InputValue value)
    {
        _currMotionVector = value.Get<Vector2>();
    }

    private void Update()
    {
        // no turning when ur fishing
        if (CurrState.Value == State.Fishing || CurrState.Value == State.Catching || CurrState.Value == State.Celebrating)
        {
            Fishing.Value = true;
            return;
        }
        Fishing.Value = false;

        if (_currMotionVector.x > 0)
        {
            FacingDir.Value= Direction.Right;
        }
        else if (_currMotionVector.x < 0)
        {
            FacingDir.Value = Direction.Left;
        }
        else if (_currMotionVector.y > 0)
        {
            FacingDir.Value = Direction.Up;
        }
        else if (_currMotionVector.y < 0)
        {
            FacingDir.Value = Direction.Down;
        }

        if (_currMotionVector.magnitude > 0)
        {
            CurrState.Value = State.Walking;
        }
        else
        {
            CurrState.Value = State.Idle;
        }
    }

    private void FixedUpdate()
    {
        // cant move when ur fishing
        if (CurrState.Value == State.Fishing || CurrState.Value == State.Catching || CurrState.Value == State.Celebrating)
        {
            return;
        }
        Vector2 _scalarMoveSpeed; 
        _scalarMoveSpeed.x = _currMotionVector.x >= 0 ? _moveSpeed.east * _moveSpeedMultiplier.east : 
                                                        _moveSpeed.west * _moveSpeedMultiplier.west;
        _scalarMoveSpeed.y = _currMotionVector.y >= 0 ? _moveSpeed.north * _moveSpeedMultiplier.north : 
                                                        _moveSpeed.south * _moveSpeedMultiplier.south;
        Vector2 _newPos = _rb.position + (_currMotionVector * Time.fixedDeltaTime * _scalarMoveSpeed);
        _rb.MovePosition(_newPos);
    }
    public void SetMoveSpeedMultiplier(CardinalVector newMultiplier) {
        _moveSpeedMultiplier = newMultiplier;
    }
}
