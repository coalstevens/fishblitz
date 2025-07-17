using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerActiveWeapon : MonoBehaviour
{
    [SerializeField] private Inventory _playerInventory;
    [SerializeField] private float _projectileSpawnRadius = 1f;
    private Reloader _reloader;
    private RangedWeaponItem _activeWeapon;
    private RangedWeaponItem.InstanceData _activeWeaponData;
    private List<Action> _unsubscribeHooks = new();

    private void OnEnable()
    {
        _reloader = GetComponent<Reloader>();
        _unsubscribeHooks.Add(_playerInventory.ActiveItemSlot.OnChange(_ => SetActiveWeapon()));

        Assert.IsNotNull(_playerInventory);
        Assert.IsNotNull(_reloader);
        SetActiveWeapon();
    }

    private void OnDisable()
    {
        foreach (var _hook in _unsubscribeHooks)
            _hook?.Invoke();
        _unsubscribeHooks.Clear();
    }

    private void SetActiveWeapon()
    {
        var activeItem = _playerInventory.GetActiveItem();
        if (activeItem == null || activeItem is not RangedWeaponItem)
        {
            _activeWeapon = null;
            _activeWeaponData = null;
            _reloader.SetActiveWeapon(null, null);
            return;
        }

        var activeItemInstanceData = _playerInventory.GetActiveItemInstanceData();
        if (activeItemInstanceData == null)
        {
            Debug.LogError("Active ranged weapon item instance data is null.");
            return;
        }

        _activeWeapon = activeItem as RangedWeaponItem;
        _activeWeaponData = activeItemInstanceData as RangedWeaponItem.InstanceData;
        _reloader.SetActiveWeapon(_activeWeapon, _activeWeaponData);

        if (_activeWeaponData != null)
        {
            // Verifying that the InstanceData was created correctly
            Assert.IsNotNull(_activeWeaponData.IsReloading);
            _activeWeaponData.ProjectileSpawnCenter = transform;
            _activeWeaponData.ProjectileSpawnRadius = _projectileSpawnRadius;
        }
    }
}
