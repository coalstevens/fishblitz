using UnityEngine;
public interface IDamaging
{
    public float DamageAmount { get; }
    public void OnHit();
}

public class Projectile : MonoBehaviour, IDamaging
{
    [SerializeField] float _damage = 1f;
    [SerializeField] float _speed = 10f;
    public float Damage
    {
        get { return _damage; }
        set { _damage = value; }
    }

    public float Speed
    {
        get { return _speed; }
        set { _speed = value; }
    }

    public float DamageAmount => throw new System.NotImplementedException();

    public void Launch(Vector3 direction)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = direction.normalized * _speed;
        }
    }

    public void OnHit()
    {
        throw new System.NotImplementedException();
    }
}
