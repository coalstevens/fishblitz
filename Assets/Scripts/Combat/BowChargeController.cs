using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

public class BowChargeController : MonoBehaviour
{
    [Header("Frame Visual")]
    [SerializeField] private Transform _rotationPivot;
    [SerializeField] private Transform _frame;
    [SerializeField] private Vector2 _framePositionLimits = new Vector2(0f, 1f);
    [SerializeField] private float _chargeTimeSecs = 1.5f;

    [Header("Projectile")]
    [SerializeField] private float _minSpeedMultiplier = 0.3f;

    [Header("References")]
    [SerializeField] private Logger _logger = new();

    private enum ChargeState { Idle, Charging }
    private ChargeState _state = ChargeState.Idle;
    private PlayerInput _playerInput;
    private InputAction _chargeAction;
    private Bow _activeBow;
    private RangedWeaponItem.InstanceData _activeWeaponData;
    private float _chargeVelocity;
    private float _chargeNormalized;
    private bool _blockNextCharge = false;
    private PlayerEnergyManager _playerEnergyManager;
    private PlayerMovementController _playerMovementController;
    [SerializeField] private PlayerData _playerData;

    private void Awake()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        Assert.IsNotNull(player, "Player not found for BowChargeController.");

        _playerInput = player.GetComponent<PlayerInput>();
        _playerEnergyManager = player.GetComponent<PlayerEnergyManager>();
        _playerMovementController = player.GetComponent<PlayerMovementController>();

        Assert.IsNotNull(_playerInput);
        Assert.IsNotNull(_playerEnergyManager);
        Assert.IsNotNull(_frame, "Frame transform is not set on BowChargeController.");

        _frame.gameObject.SetActive(false);
    }

    public bool StartCharge(Bow bow, RangedWeaponItem.InstanceData weaponData)
    {
        if (_state != ChargeState.Idle) return false;
        if (_blockNextCharge) return false;
        if (_playerData != null && _playerData.IsCarrying.Value) return false;

        _activeBow = bow;
        _activeWeaponData = weaponData;

        _playerInput.SwitchCurrentActionMap("Combat");
        _chargeAction = _playerInput.actions["UseTool"];
        Assert.IsNotNull(_chargeAction, "UseTool action not found in Combat map.");

        _chargeVelocity = (_framePositionLimits.y - _framePositionLimits.x) / _chargeTimeSecs;

        _frame.localPosition = new Vector3(_framePositionLimits.x, 0f, 0f);
        _frame.gameObject.SetActive(true);

        if (!_activeBow.AllowMovementWhileCharging)
        {
            _playerMovementController.PlayerState.Value = PlayerMovementController.PlayerStates.BowCharging;
        }

        _state = ChargeState.Charging;
        _chargeNormalized = 0f;

        _logger.Info("Bow charging started");
        return true;
    }

    private void Update()
    {
        if (_state == ChargeState.Idle) return;

        AlignPivotToMouse();

        if (_chargeAction.IsPressed())
        {
            float newX = _frame.localPosition.x + _chargeVelocity * Time.deltaTime;
            if (newX >= _framePositionLimits.y)
            {
                _frame.localPosition = new Vector3(_framePositionLimits.y, 0f, 0f);
                _chargeNormalized = 1f;
                Fire();
            }
            else
            {
                _frame.localPosition = new Vector3(newX, 0f, 0f);
                _chargeNormalized = (newX - _framePositionLimits.x) / (_framePositionLimits.y - _framePositionLimits.x);
            }
        }
        else if (_chargeNormalized > 0f)
        {
            Fire();
        }
    }

    private void Fire()
    {
        if (_activeWeaponData == null) return;

        if (_playerEnergyManager != null && _activeBow is PlayerEnergyManager.IEnergyDepleting energyDepleting)
        {
            if (!_playerEnergyManager.IsSufficientEnergyAvailable(energyDepleting))
            {
                EndCharge();
                return;
            }
        }

        float speedMultiplier = _minSpeedMultiplier + (1f - _minSpeedMultiplier) * _chargeNormalized;

        Vector2 spawnCenter = _activeWeaponData.ProjectileSpawnCenter.position;
        float spawnRadius = _activeWeaponData.ProjectileSpawnRadius;

        Vector2 targetPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 direction = (targetPosition - spawnCenter).normalized;
        Vector2 spawnPosition = spawnCenter + direction * spawnRadius;

        GameObject projectileObj = ObjectPooling.SpawnObject(_activeBow.ProjectilePrefab, spawnPosition, Quaternion.identity);
        projectileObj.transform.localRotation = Quaternion.FromToRotation(Vector2.left, direction);

        Projectile projectile = projectileObj.GetComponent<Projectile>();
        projectile.Launch(direction, _activeBow.IsEnemyProjectile, speedMultiplier);

        if (_playerEnergyManager != null && _activeBow is PlayerEnergyManager.IEnergyDepleting deplete)
        {
            _playerEnergyManager.DepleteEnergy(deplete.EnergyCost);
        }

        _activeWeaponData.IsCoolingDown.Value = true;
        _activeWeaponData.CurrentClipCount--;
        if (_activeWeaponData.CurrentClipCount <= 0)
        {
            _activeWeaponData.IsReloading.Value = true;
        }

        _logger.Info($"Bow fired with charge {_chargeNormalized:F2}");
        EndCharge();
    }

    private void EndCharge()
    {
        _frame.gameObject.SetActive(false);
        _frame.localPosition = new Vector3(_framePositionLimits.x, 0f, 0f);

        if (_playerMovementController.PlayerState.Value == PlayerMovementController.PlayerStates.BowCharging)
        {
            _playerMovementController.PlayerState.Value = PlayerMovementController.PlayerStates.Idle;
        }

        if (_rotationPivot != null)
            _rotationPivot.localRotation = Quaternion.identity;

        _playerInput.SwitchCurrentActionMap("Player");
        _state = ChargeState.Idle;
        _chargeNormalized = 0f;
        _activeBow = null;
        _activeWeaponData = null;

        _logger.Info("Bow charge ended");
        StartCoroutine(ChargeCooldown());
    }

    private IEnumerator ChargeCooldown()
    {
        _blockNextCharge = true;
        yield return null;
        _blockNextCharge = false;
    }

    private void AlignPivotToMouse()
    {
        if (_rotationPivot == null) return;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 dir = mouseWorld - _rotationPivot.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        _rotationPivot.localEulerAngles = new Vector3(0f, 0f, angle);
    }

    public void AbortCharge()
    {
        if (_state == ChargeState.Idle) return;
        EndCharge();
    }
}
