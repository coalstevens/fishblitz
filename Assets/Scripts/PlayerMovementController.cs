using UnityEngine;
using UnityEngine.InputSystem;

public enum Direction
{
    Up,
    Down,
    Left,
    Right,
}

public class PlayerMovementController : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5f;
    private Vector2 _currMotionVector = Vector2.zero;
    private Rigidbody2D _rb;

    private Reactive<bool> _fishing = new Reactive<bool>(false);
    public Reactive<Direction> FacingDir = new Reactive<Direction>(Direction.Up);

    public bool Fishing
    {
        get
        {
            return _fishing.Get();
        }

        set
        {
            _fishing.Set(value);
        }
    }

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
        if (_currMotionVector.x > 0)
        {
            FacingDir.Set(Direction.Right);
        }
        else if (_currMotionVector.x < 0)
        {
            FacingDir.Set(Direction.Left);
        }
        else if (_currMotionVector.y > 0)
        {
            FacingDir.Set(Direction.Up);
        }
        else if (_currMotionVector.y < 0)
        {
            FacingDir.Set(Direction.Down);
        }
    }

    private void FixedUpdate()
    {
        // cant move when ur fishing
        if (Fishing)
        {
            return;
        }

        Vector2 newPos = _rb.position + (_currMotionVector * Time.fixedDeltaTime * _moveSpeed);
        _rb.MovePosition(newPos);
    }
}
