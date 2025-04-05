using System;
using ReactiveUnity;
using UnityEngine;
using UnityEngine.Assertions;

[CreateAssetMenu(fileName = "NewRangedWeapon", menuName = "Combat/RangedWeapon")]
[Serializable]
public class RangedWeaponItem : Inventory.Item, Inventory.IInstancedItem<RangedWeaponItem.InstanceData>
{
    public enum FireTypes { SingleShot }
    public FireTypes FireType;
    public float FireRateShotsPerSec;
    public float ReloadTime;
    public int ClipSize;
    public GameObject ProjectilePrefab;

    [Serializable]
    public class InstanceData : Inventory.ItemInstanceData
    {
        [HideInInspector] public Transform ProjectileSpawnCenter;
        [HideInInspector] public float ProjectileSpawnRadius;
        [HideInInspector] public Reactive<bool> IsReloading;
        [HideInInspector] public Reactive<bool> IsCoolingDown;
        [HideInInspector] public float ReloadElapsed;
        [HideInInspector] public float CoolDownElapsed;
        [HideInInspector] public int CurrentClipCount;
        [HideInInspector] public Collider2D TargetCollider;
    }

    public InstanceData CreateInstanceData()
    {
        InstanceData _instanceData = new InstanceData
        {
            ProjectileSpawnCenter = null,
            ProjectileSpawnRadius = 0,
            IsReloading = new Reactive<bool>(false),
            IsCoolingDown = new Reactive<bool>(false),
            ReloadElapsed = 0f,
            CoolDownElapsed = 0f,
            CurrentClipCount = ClipSize,
            TargetCollider = null
        };
        return _instanceData;
    }

    private void OnEnable()
    {
        Assert.IsTrue(ClipSize > 0);
        Assert.IsTrue(FireRateShotsPerSec > 0);
        Assert.IsNotNull(ProjectilePrefab);
    }

    public bool TryFire(InstanceData instanceData)
    {
        Assert.IsNotNull(instanceData);
        if (instanceData.IsReloading.Value || instanceData.IsCoolingDown.Value)
            return false;

        ExecuteFireMethod(instanceData);

        instanceData.IsCoolingDown.Value = true;
        instanceData.CurrentClipCount--;
        if (instanceData.CurrentClipCount <= 0)
        {
            instanceData.IsReloading.Value = true;
        }
        return true;
    }

    private void ExecuteFireMethod(InstanceData instanceData)
    {
        Assert.IsNotNull(instanceData);
        switch (FireType)
        {
            case FireTypes.SingleShot:
                FireSingleShot(ProjectilePrefab, instanceData.ProjectileSpawnCenter.position, instanceData.ProjectileSpawnRadius, instanceData.TargetCollider);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void FireSingleShot(GameObject projectilePrefab, Vector2 projectileSpawnCenter, float projectileSpawnRadius, Collider2D target)
    {
        Assert.IsNotNull(projectilePrefab);

        Vector2 _targetPosition;
        if (target == null)
            _targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        else
            _targetPosition = (Vector2)target.transform.position + target.offset;

        Vector2 _targetDirection = (_targetPosition - projectileSpawnCenter).normalized;
        Vector2 _projectileSpawnPosition = projectileSpawnCenter + _targetDirection * projectileSpawnRadius;

        GameObject _projectileObject = ObjectPooling.SpawnObject(projectilePrefab, _projectileSpawnPosition, Quaternion.identity); 
        _projectileObject.transform.localRotation = Quaternion.FromToRotation(Vector2.left, _targetDirection);

        Projectile _projectile = _projectileObject.GetComponent<Projectile>();
        _projectile.Launch(_targetDirection);
    }
}
