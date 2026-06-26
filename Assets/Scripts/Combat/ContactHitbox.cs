using System;
using UnityEngine;
using UnityEngine.Assertions;

public class ContactHitbox : MonoBehaviour
{
    [SerializeField] private float _damage = 1f;
    [SerializeField] private KnockbackMode _knockbackMode = KnockbackMode.None;
    [SerializeField] private float _knockbackForce = 15f;
    [SerializeField] private Logger _logger = new();

    public event Action<IHurtBox> OnHit;

    private Rigidbody2D _parentRb;
    public float Damage => _damage;
    public void SetDamage(float damage) => _damage = damage;

    private int _friendlyHurtboxLayer;
    private int _enemyHurtboxLayer;
    private int _friendlyHitboxLayer;
    private int _enemyHitboxLayer;

    private void Awake()
    {
        _parentRb = GetComponentInParent<Rigidbody2D>();
        _friendlyHurtboxLayer = LayerMask.NameToLayer("FriendlyHurtbox");
        _enemyHurtboxLayer = LayerMask.NameToLayer("EnemyHurtbox");
        _friendlyHitboxLayer = LayerMask.NameToLayer("FriendlyHitbox");
        _enemyHitboxLayer = LayerMask.NameToLayer("EnemyHitbox");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        IHurtBox hurtbox = other.GetComponent<IHurtBox>();
        if (hurtbox == null || !hurtbox.IsVulnerable)
            return;

        if (!IsValidTarget(other))
            return;

        _logger.Info($"Dealt {_damage} damage to {other.name}");
        hurtbox.TakeDamage(_damage);
        ApplyKnockback(other);
        OnHit?.Invoke(hurtbox);
    }

    private bool IsValidTarget(Collider2D other)
    {
        int otherLayer = other.gameObject.layer;
        int ownLayer = gameObject.layer;

        if (ownLayer == _enemyHitboxLayer)
            return otherLayer == _friendlyHurtboxLayer;
        if (ownLayer == _friendlyHitboxLayer)
            return otherLayer == _enemyHurtboxLayer;

        return otherLayer == _friendlyHurtboxLayer || otherLayer == _enemyHurtboxLayer;
    }

    private void ApplyKnockback(Collider2D other)
    {
        if (_knockbackForce <= 0f || _knockbackMode == KnockbackMode.None)
            return;

        switch (_knockbackMode)
        {
            case KnockbackMode.KnockSelf:
                DoKnockback(_parentRb, other.transform.position, _knockbackForce);
                break;
            case KnockbackMode.KnockTarget:
                DoKnockback(GetTargetRb(other), transform.position, _knockbackForce);
                break;
            case KnockbackMode.KnockBoth:
                DoKnockback(_parentRb, other.transform.position, _knockbackForce);
                DoKnockback(GetTargetRb(other), transform.position, _knockbackForce);
                break;
        }
    }

    private static Rigidbody2D GetTargetRb(Collider2D other)
    {
        return other.GetComponentInParent<Rigidbody2D>() ?? other.attachedRigidbody;
    }

    private static void DoKnockback(Rigidbody2D rb, Vector2 sourcePosition, float force)
    {
        if (rb == null) return;
        Vector2 direction = ((Vector2)rb.transform.position - sourcePosition).normalized;
        rb.AddForce(direction * force, ForceMode2D.Impulse);
    }
}
