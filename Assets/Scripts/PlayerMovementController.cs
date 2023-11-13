using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementController : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5f;
    private Vector2 _currMotionVector = Vector2.zero;
    private Rigidbody2D _rb;

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
        Vector2 newPos = _rb.position + (_currMotionVector * Time.fixedDeltaTime * _moveSpeed);
        _rb.MovePosition(newPos);
    }
}
