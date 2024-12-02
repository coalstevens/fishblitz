using UnityEngine;

public class Bird : MonoBehaviour
{
    [SerializeField] public string Name = "Chickadee";
    [SerializeField] public Sprite Icon;
    [SerializeField] public bool Caught = false;

    Rigidbody2D _rb;

    private void Awake() {
        _rb = GetComponent<Rigidbody2D>();
    }

    public Vector2 GetVelocity() {
        return _rb.velocity;
    }
}

