using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementController : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5f;
    private Vector2 _currMotionVector = Vector2.zero;
    private Rigidbody2D _rb;

    private Reactive<bool> _fishing = new Reactive<bool>(false);
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
