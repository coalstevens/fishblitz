using System.Collections;
using ReactiveUnity;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(PlayerCarry), typeof(PlayerWheelBarrow))]
public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float _maxHealth = 5f;
    [SerializeField] private float _invulnerabilityDuration = 1.5f;
    [SerializeField] private float _flashDuration = 0.1f;

    [Header("Death Respawn")]
    [SerializeField] private SceneNames _respawnScene = SceneNames.CanyonStart;
    [SerializeField] private Vector3 _respawnPosition = Vector3.zero;

    [Header("Debug")]
    [SerializeField] private Logger _logger = new();

    private PlayerCarry _playerCarry;
    private PlayerWheelBarrow _playerWheelBarrow;
    private PlayerAnimatorController _animatorController;
    private bool _hasDied;

    private static bool _restoreHealthOnRespawn;
    private static float _healthOnRespawn;

    public Reactive<float> CurrentHealth = new Reactive<float>(0f);
    public Reactive<bool> IsInvulnerable = new Reactive<bool>(false);

    public SceneNames RespawnScene => _respawnScene;
    public Vector3 RespawnPosition => _respawnPosition;

    private void Awake()
    {
        _playerCarry = GetComponent<PlayerCarry>();
        _playerWheelBarrow = GetComponent<PlayerWheelBarrow>();
        _animatorController = GetComponentInChildren<PlayerAnimatorController>();
    }

    private void OnEnable()
    {
        if (_restoreHealthOnRespawn)
        {
            _restoreHealthOnRespawn = false;
            CurrentHealth.Value = _healthOnRespawn;
            _logger.Info($"Player health restored to {CurrentHealth.Value}");
        }
        else
        {
            CurrentHealth.Value = _maxHealth;
            _logger.Info($"Player health set to max: {CurrentHealth.Value}");
        }
    }

    public void TakeDamage(float damage)
    {
        if (IsInvulnerable.Value)
            return;

        Assert.IsTrue(damage > 0);

        if (TryAbsorbDamageByDropping())
        {
            StartCoroutine(InvulnerabilityFrames());
            return;
        }

        _logger.Info($"Player took {damage} damage, health now {CurrentHealth.Value - damage}");
        CurrentHealth.Value -= damage;
        if (CurrentHealth.Value <= 0)
        {
            CurrentHealth.Value = 0;
            Die();
            return;
        }

        StartCoroutine(InvulnerabilityFrames());
        StartCoroutine(FlashRedTwice());
    }

    private bool TryAbsorbDamageByDropping()
    {
        if (_playerCarry.IsCarrying.Value)
            return _playerCarry.DropCarriedItemOnHit();

        if (_playerWheelBarrow.IsHoldingWheelBarrow.Value && !_playerWheelBarrow.WheelBarrowStack.IsEmpty())
            return _playerCarry.DropStackItemOnHit(_playerWheelBarrow.WheelBarrowStack);

        return false;
    }

    private IEnumerator InvulnerabilityFrames()
    {
        IsInvulnerable.Value = true;
        yield return new WaitForSeconds(_invulnerabilityDuration);
        IsInvulnerable.Value = false;
    }

    private IEnumerator FlashRedTwice()
    {
        SpriteRenderer[] _renderers = GetActiveSpriteRenderers();
        if (_renderers.Length == 0)
            yield break;

        Color[] _originalColors = new Color[_renderers.Length];
        for (int i = 0; i < _renderers.Length; i++)
            _originalColors[i] = _renderers[i].color;

        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < _renderers.Length; j++)
                _renderers[j].color = Color.red;
            yield return new WaitForSeconds(_flashDuration);

            for (int j = 0; j < _renderers.Length; j++)
                _renderers[j].color = _originalColors[j];
            yield return new WaitForSeconds(_flashDuration);
        }
    }

    private SpriteRenderer[] GetActiveSpriteRenderers()
    {
        if (_animatorController == null || _animatorController.ActiveSprite == null)
            return System.Array.Empty<SpriteRenderer>();

        return _animatorController.ActiveSprite.GetComponentsInChildren<SpriteRenderer>(true);
    }

    private void Die()
    {
        if (_hasDied)
            return;
        _hasDied = true;

        Narrator.Instance?.PostMessage("i'm hurt bad...");
        PlayerSceneData.PendingSpawnPosition = _respawnPosition;
        PlayerSceneData.HasPendingSpawn = true;
        LevelChanger.ChangeLevel(_respawnScene);
    }

    public void RespawnAtDeathPosition()
    {
        _restoreHealthOnRespawn = true;
        _healthOnRespawn = CurrentHealth.Value;
        PlayerSceneData.PendingSpawnPosition = _respawnPosition;
        PlayerSceneData.HasPendingSpawn = true;
        LevelChanger.ChangeLevel(_respawnScene);
    }
}
