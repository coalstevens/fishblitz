using System.Collections;
using ReactiveUnity;
using UnityEngine;
using UnityEngine.Assertions;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private float _maxHealth = 2f;
    [SerializeField] private float _invulnerabilityDuration = 1.5f;
    public Reactive<float> CurrentHealth = new Reactive<float>(0);
    public Reactive<bool> IsInvulnerable = new(false);

    private void OnEnable()
    {
        CurrentHealth.Value = _maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (IsInvulnerable.Value) return;
        Assert.IsTrue(damage > 0);

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

    public void Heal(int amount)
    {
        Assert.IsTrue(amount > 0);

        CurrentHealth.Value += amount;
        if (CurrentHealth.Value > _maxHealth)
        {
            CurrentHealth.Value = _maxHealth;
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} has died.");
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        if (CurrentHealth == null) return;

        Vector3 pos = transform.position + Vector3.up * 2f;
        float barWidth = 1f;
        float barHeight = 0.15f;
        float pct = CurrentHealth.Value / _maxHealth;

        Gizmos.color = Color.red;
        Gizmos.DrawCube(pos, new Vector3(barWidth, barHeight, 0f));

        Gizmos.color = Color.green;
        Vector3 offset = Vector3.left * (barWidth * (1f - pct) / 2f);
        Gizmos.DrawCube(pos + offset, new Vector3(barWidth * pct, barHeight, 0f));
    }
}
