using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

public interface IHurtBox
{
    public bool IsCovered { get; }
    public void TakeDamage();
}

public class Projectile : MonoBehaviour
{
    [SerializeField] private float _lifespan;
    [SerializeField] private float _speed;
    [SerializeField] private float _damager;
    [SerializeField] private Logger _logger = new();
    private Rigidbody2D _rb;
    private bool _crossedCover = false;
    public bool IsFriendly = false;
    private Coroutine _lifespanTimeoutCoroutine;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        Assert.IsNotNull(_rb);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        _logger.Info($"Projectile entered: {collision.gameObject.name}");
        
        if (IsColliderInLayer(collision, "Default"))
        {
            _logger.Info("  Contacted default object. Disabling projectile.");
            DisableProjectile();
            return;
        }

        if (IsColliderInLayer(collision, "Cover"))
        {
            _logger.Info("  Entered cover effective area.");
            if (!collision.isTrigger)
            {
                _crossedCover = true;
            }
            return;
        }

        if (IsColliderInLayer(collision, "FriendlyHurtbox") && !IsFriendly ||
            IsColliderInLayer(collision, "EnemyHurtbox") && IsFriendly)
        {
            _logger.Info("  Entered friendly hurtbox.");
            IHurtBox _hurtbox = collision.transform.GetComponent<IHurtBox>();
            if (!_hurtbox.IsCovered || !_crossedCover)
            {
                _logger.Info("  Hurtbox is not covered. Inflicting damage.");
                _hurtbox.TakeDamage();
                DisableProjectile();
            }
            return;
        }
    }

    private bool IsColliderInLayer(Collider2D collider, string layerName)
    {
        return collider.gameObject.layer == LayerMask.NameToLayer(layerName);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Cover") && collision.isTrigger)
        {
            // left effective area of cover
            if (_crossedCover)
                _crossedCover = false;
        }
    }

    public void Launch(Vector2 direction)
    {
        _lifespanTimeoutCoroutine = StartCoroutine(DisableProjectileAfterLifespan());
        if (_rb != null)
        {
            _rb.linearVelocity = direction.normalized * _speed;
        }
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
        _rb.linearVelocity = Vector2.zero;
        ObjectPooling.ReturnObjectToPool(this.gameObject);
    }

}
