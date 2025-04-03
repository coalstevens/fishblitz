using UnityEngine;
using UnityEngine.Assertions;

public interface IHurtBox
{
    public bool IsCovered { get; }
    public void TakeDamage();
}

public class Projectile : MonoBehaviour
{
    [SerializeField] private Logger _logger = new();
    private Rigidbody2D _rb;
    private IWeapon _sourceWeapon;
    private bool _crossedCover = false;
    private float _damage;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _sourceWeapon = transform.parent.GetComponent<IWeapon>();
        Assert.IsNotNull(_rb);
        Assert.IsNotNull(_sourceWeapon);

        _damage = _sourceWeapon.Damage;
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        _logger.Info($"Projectile entered: {collision.gameObject.name}");
        if (collision.gameObject.layer == LayerMask.NameToLayer("Default"))
        {
            _logger.Info("  Contacted default object. Disabling projectile.");
            DisableProjectile();
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("Cover"))
        {
            _logger.Info("  Entered cover effective area.");
            if (!collision.isTrigger)
            {
                _crossedCover = true;
            }
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("FriendlyHurtbox"))
        {
            _logger.Info("  Entered friendly hurtbox.");
            IHurtBox _hurtbox = collision.transform.GetComponent<IHurtBox>();
            if (!_hurtbox.IsCovered || !_crossedCover)
            {
                _logger.Info("  Hurtbox is not covered. Inflicting damage.");
                _hurtbox.TakeDamage();
                DisableProjectile();
            }
        }
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

    public void Launch(Vector2 direction, float speed, float lifeSpan)
    {
        gameObject.SetActive(true);
        Invoke(nameof(DisableProjectile), lifeSpan);
        if (_rb != null)
        {
            _rb.linearVelocity = direction.normalized * speed;
        }
    }

    public void DisableProjectile()
    {
        _rb.linearVelocity = Vector2.zero;
        gameObject.SetActive(false);
    }
}
