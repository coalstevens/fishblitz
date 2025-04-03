using System;
using UnityEngine;
using UnityEngine.Assertions;

public class EnemyCombatStatus : MonoBehaviour, IHealth
{
    [SerializeField] private EnemyCombatStatusData _statusData;
    public float CurrentHealth => _currentHealth;
    public float MaxHealth => _statusData.MaxHealth;
    private float _currentHealth;
    public event Action OnHealthUpdate;

    private void OnEnable()
    {
        Assert.IsNotNull(_statusData);
        _currentHealth = _statusData.MaxHealth;
    }

    public void TakeDamage(float damage)
    {
        Assert.IsTrue(damage > 0);
        OnHealthUpdate.Invoke();

        _currentHealth -= damage;
        if (_currentHealth <= 0)
        {
            _currentHealth = 0;
            Die();
        }
    }

    public void Heal(int amount)
    {
        Assert.IsTrue(amount > 0);
        OnHealthUpdate.Invoke();

        _currentHealth += amount;
        if (_currentHealth > MaxHealth)
        {
            _currentHealth = MaxHealth;
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} has died.");
    }
}
