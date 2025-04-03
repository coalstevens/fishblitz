using System;
using ReactiveUnity;
using UnityEngine;
using UnityEngine.Assertions;

public interface IWeapon
{
    public float Damage { get; } 
    public float ReloadElapsedSecs { get; }
    public float ReloadTimeSecs { get; }
    public event Action OnReloadComplete;
    public event Action OnReloadStart;
}

public class EnemyWeapon : MonoBehaviour, IWeapon 
{
    [SerializeField] private Collider2D _targetCollider;
    [SerializeField] private BasicWeapon _basicWeapon;
    [SerializeField] private float _projectileSpawnRadius = 1f;
    public float Damage => _basicWeapon.Damage;
    public event Action OnReloadComplete;
    public event Action OnReloadStart;
    public float ReloadElapsedSecs => _reloadElaspedSecs;
    public float ReloadTimeSecs => 1f / _basicWeapon.FireRate;

    private float _reloadElaspedSecs; 

    private void Start()
    {
        Assert.IsNotNull(_targetCollider);
        Assert.IsNotNull(_basicWeapon);
        SetSpiteOnProjectiles();
    }

    private void FixedUpdate()
    {
        _reloadElaspedSecs += Time.deltaTime;
        if (_reloadElaspedSecs >= ReloadTimeSecs)
        {
            _reloadElaspedSecs = 0;
            OnReloadComplete.Invoke(); 
            Fire();
        }
    }

    private void Fire()
    {
        Vector2 _targetCenter = (Vector2)_targetCollider.transform.position + _targetCollider.offset;
        Vector2 _targetDirection = (_targetCenter - (Vector2)transform.position).normalized;
        Vector2 _projectileSpawnPosition = (Vector2)transform.position + _targetDirection * _projectileSpawnRadius;

        Projectile _projectile = GetNextProjectile();
        if (_projectile == null)
            return;

        _projectile.transform.position = _projectileSpawnPosition;
        _projectile.Launch(_targetDirection, _basicWeapon.ProjectileSpeed, _basicWeapon.ProjectileLifespan);
        OnReloadStart.Invoke();
    }

    private Projectile GetNextProjectile()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;
            if (!child.activeInHierarchy)
            {
                Projectile _projectile = child.GetComponent<Projectile>();
                Assert.IsNotNull(_projectile);
                return _projectile;
            }
        }
        Debug.LogError("There are not enough projectiles in the pool to account for the firerate");
        return null;
    }

    private void SetSpiteOnProjectiles()
    {
        foreach (Transform _child in transform)
            _child.GetComponent<SpriteRenderer>().sprite = _basicWeapon.ProjectileSprite;
    }

}
