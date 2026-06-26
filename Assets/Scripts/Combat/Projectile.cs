using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float _lifespan;
    [SerializeField] private float _speed;

    private Rigidbody2D _rb;
    private ContactHitbox _hitbox;
    private SpriteRenderer _spriteRenderer;
    private float _baseDamage;
    private bool _isCrit;
    private Coroutine _lifespanTimeoutCoroutine;

    private void Awake()
    {
        Assert.IsTrue(_lifespan > 0);

        _rb = GetComponent<Rigidbody2D>();
        Assert.IsNotNull(_rb);

        _spriteRenderer = GetComponent<SpriteRenderer>();
        Assert.IsNotNull(_spriteRenderer);

        _hitbox = GetComponentInChildren<ContactHitbox>();
        Assert.IsNotNull(_hitbox);
        _baseDamage = _hitbox.Damage;
        _hitbox.OnHit += OnHitTarget;
    }

    private void OnDestroy()
    {
        if (_hitbox != null)
            _hitbox.OnHit -= OnHitTarget;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Default"))
            DisableProjectile();
    }

    public void Launch(Vector2 direction, float speedMultiplier = 1f)
    {
        _lifespanTimeoutCoroutine = StartCoroutine(DisableProjectileAfterLifespan());

        if (_rb != null)
            _rb.linearVelocity = direction.normalized * _speed * speedMultiplier;
    }

    public void SetCrit(bool value)
    {
        _isCrit = value;
        if (value)
        {
            _spriteRenderer.color = Color.yellow;
            _hitbox.SetDamage(_baseDamage * 2);
        }
        else
        {
            _spriteRenderer.color = Color.white;
            _hitbox.SetDamage(_baseDamage);
        }
    }

    private void OnHitTarget(IHurtBox _)
    {
        DisableProjectile();
    }

    private IEnumerator DisableProjectileAfterLifespan()
    {
        yield return new WaitForSeconds(_lifespan);
        DisableProjectile();
    }

    private void DisableProjectile()
    {
        if (_lifespanTimeoutCoroutine != null)
        {
            StopCoroutine(_lifespanTimeoutCoroutine);
            _lifespanTimeoutCoroutine = null;
        }

        if (_rb != null)
            _rb.linearVelocity = Vector2.zero;

        SetCrit(false);
        ObjectPooling.ReturnObjectToPool(gameObject);
    }
}
