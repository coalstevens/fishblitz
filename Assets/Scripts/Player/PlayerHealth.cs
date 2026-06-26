using System.Collections;
using ReactiveUnity;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float _maxHealth = 5f;
    [SerializeField] private float _invulnerabilityDuration = 1.5f;

    [Header("Debug")]
    [SerializeField] private Logger _logger = new();

    public Reactive<float> CurrentHealth = new Reactive<float>(0f);
    public Reactive<bool> IsInvulnerable = new Reactive<bool>(false);

    private void OnEnable()
    {
        CurrentHealth.Value = _maxHealth;
        _logger.Info($"Player health set to max: {CurrentHealth.Value}");
    }

    public void TakeDamage(float damage)
    {
        if (IsInvulnerable.Value)
            return;

        Assert.IsTrue(damage > 0);

        _logger.Info($"Player took {damage} damage, health now {CurrentHealth.Value - damage}");
        CurrentHealth.Value -= damage;
        if (CurrentHealth.Value <= 0)
        {
            CurrentHealth.Value = 0;
            Die();
            return;
        }

        StartCoroutine(InvulnerabilityFrames());
    }

    private IEnumerator InvulnerabilityFrames()
    {
        IsInvulnerable.Value = true;
        yield return new WaitForSeconds(_invulnerabilityDuration);
        IsInvulnerable.Value = false;
    }

    private void Die()
    {
        Narrator.Instance?.PostMessage("i'm hurt bad...");
    }
}
