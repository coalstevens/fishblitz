using UnityEngine;
using UnityEngine.Assertions;

public class EnemyActiveWeapon : MonoBehaviour 
{
    [SerializeField] private Collider2D _targetCollider;
    [SerializeField] private RangedWeaponItem _weapon;
    [SerializeField] private float _projectileSpawnRadius;
    [SerializeField] private bool _isFiring = true;
    private Reloader _reloader;
    private RangedWeaponItem.InstanceData _weaponData;

    private void Start()
    {
        Assert.IsNotNull(_targetCollider);
        Assert.IsNotNull(_weapon);

        _weaponData = _weapon.CreateInstanceData();
        _weaponData.TargetCollider = _targetCollider;
        _weaponData.ProjectileSpawnCenter = transform;
        _weaponData.ProjectileSpawnRadius = _projectileSpawnRadius; 

        _reloader = GetComponent<Reloader>();
        Assert.IsNotNull(_reloader);

        _reloader.SetActiveWeapon(_weapon, _weaponData);
    }

    private void Update()
    {
        if (_isFiring)
            _weapon.TryFire(_weaponData);
    }
}
