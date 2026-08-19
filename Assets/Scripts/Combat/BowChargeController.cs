using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

public class BowChargeController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Logger _logger = new();

    private enum ChargeState { Idle, Charging }
    private ChargeState _state = ChargeState.Idle;
    private PlayerInput _playerInput;
    private InputAction _chargeAction;
    private Bow _activeBow;
    private RangedWeaponItem.InstanceData _activeWeaponData;
    private float _chargeNormalized;
    private bool _blockNextCharge = false;
    private PlayerEnergyManager _playerEnergyManager;
    private PlayerMovement _playerMovementController;
    private PlayerCarry _playerCarry;
    private BowChargeView _view;

    private void Awake()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        Assert.IsNotNull(player, "Player not found for BowChargeController.");

        _playerCarry = player.GetComponent<PlayerCarry>();
        _playerInput = player.GetComponent<PlayerInput>();
        _playerEnergyManager = player.GetComponent<PlayerEnergyManager>();
        _playerMovementController = player.GetComponent<PlayerMovement>();

        Assert.IsNotNull(_playerInput);
        Assert.IsNotNull(_playerEnergyManager);

        _view = GetComponent<BowChargeView>();
        Assert.IsNotNull(_view, "BowChargeView not found on BowChargeController GameObject.");
    }

    public bool StartCharge(Bow bow, RangedWeaponItem.InstanceData weaponData)
    {
        if (_state != ChargeState.Idle) return false;
        if (_blockNextCharge) return false;
        if (_playerCarry != null && _playerCarry.IsCarrying.Value) return false;
        if (weaponData.IsReloading.Value || weaponData.IsCoolingDown.Value) return false;

        _activeBow = bow;
        _activeWeaponData = weaponData;

        _playerInput.SwitchCurrentActionMap("Combat");
        _chargeAction = _playerInput.actions["UseTool"];
        Assert.IsNotNull(_chargeAction, "UseTool action not found in Combat map.");

        _view.ShowCharge();

        if (!_activeBow.AllowMovementWhileCharging)
        {
            _playerMovementController.PlayerState.Value = PlayerMovement.PlayerStates.BowCharging;
        }

        _state = ChargeState.Charging;
        _chargeNormalized = 0f;

        _logger.Info("Bow charging started");
        return true;
    }

    private void Update()
    {
        if (_state == ChargeState.Idle) return;

        _view.AlignPivotToMouse();

        if (_chargeAction.IsPressed())
        {
            _chargeNormalized += Time.deltaTime / _activeBow.ChargeTimeSecs;
            if (_chargeNormalized >= 1f)
            {
                _chargeNormalized = 1f;
                _view.SetChargeNormalized(1f);
                Fire();
                return;
            }
            _view.SetChargeNormalized(_chargeNormalized);
        }
        else if (_chargeNormalized > 0f)
        {
            if (_chargeNormalized >= _activeBow.MinChargeNormalized)
                Fire();
            else
                EndCharge();
            return;
        }

        _view.UpdateChargeAlpha(_chargeNormalized, _activeBow.MinChargeNormalized);
        _view.UpdateCritVisual(_chargeNormalized, _activeBow.CritShotCharge, _activeBow.MinChargeNormalized);
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

        float t = Mathf.InverseLerp(_activeBow.MinChargeNormalized, 1f, _chargeNormalized);
        float speedMultiplier = Mathf.Lerp(_activeBow.MinSpeedMultiplier, 1f, t);

        Vector2 spawnCenter = _activeWeaponData.ProjectileSpawnCenter.position;
        float spawnRadius = _activeWeaponData.ProjectileSpawnRadius;

        Vector2 targetPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 direction = (targetPosition - spawnCenter).normalized;
        Vector2 spawnPosition = spawnCenter + direction * spawnRadius;

        GameObject projectileObj = ObjectPooling.SpawnObject(_activeBow.ProjectilePrefab, spawnPosition, Quaternion.identity);
        projectileObj.transform.localRotation = Quaternion.FromToRotation(Vector2.left, direction);

        Projectile projectile = projectileObj.GetComponent<Projectile>();
        projectile.Launch(direction, speedMultiplier);

        bool isCrit = _chargeNormalized >= _activeBow.CritShotCharge.x
                   && _chargeNormalized < _activeBow.CritShotCharge.y;
        if (isCrit) projectile.SetCrit(true);

        if (_playerEnergyManager != null && _activeBow is PlayerEnergyManager.IEnergyDepleting deplete)
        {
            _playerEnergyManager.DepleteEnergy(deplete.EnergyCost);
        }

        _activeWeaponData.RecordShot();

        _logger.Info($"Bow fired with charge {_chargeNormalized:F2}");
        EndCharge();
    }

    private void EndCharge()
    {
        _view.HideCharge();

        if (_playerMovementController.PlayerState.Value == PlayerMovement.PlayerStates.BowCharging ||
            _playerMovementController.PlayerState.Value == PlayerMovement.PlayerStates.BowChargingRunning)
        {
            _playerMovementController.PlayerState.Value = PlayerMovement.PlayerStates.Idle;
        }

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

    public void AbortCharge()
    {
        if (_state == ChargeState.Idle) return;
        EndCharge();
    }
}
