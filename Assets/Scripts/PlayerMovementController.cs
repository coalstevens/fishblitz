using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using ReactiveUnity;
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

public class PlayerMovementController : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5f;
    private Vector2 _currMotionVector = Vector2.zero;
    private Rigidbody2D _rb;

    //TODO change _fishing reactive to "Acting"? cursor should disappear in an action state/animation like fishing
    // _fishing currently just toggles the cursor display
    public Reactive<bool> _fishing = new Reactive<bool>(false);
    public Reactive<Direction> FacingDir = new Reactive<Direction>(Direction.Up);
    public Reactive<State> CurrState = new Reactive<State>(State.Idle);

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
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
            _fishing.Value = true;
            return;
        }
        _fishing.Value = false;

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

        Vector2 newPos = _rb.position + (_currMotionVector * Time.fixedDeltaTime * _moveSpeed);
        _rb.MovePosition(newPos);
    }
}
