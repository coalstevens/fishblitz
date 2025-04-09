using ReactiveUnity;
using UnityEngine;
using UnityEngine.Assertions;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private EnemyHealthData _statusData;
    public Reactive<float> CurrentHealth = new Reactive<float>(0);
    public float MaxHealth => _statusData.MaxHealth;

    private void OnEnable()
    {
        Assert.IsNotNull(_statusData);
        CurrentHealth.Value = _statusData.MaxHealth;
    }

    public void TakeDamage(float damage)
    {
        Assert.IsTrue(damage > 0);

        CurrentHealth.Value -= damage;
        if (CurrentHealth.Value <= 0)
        {
            CurrentHealth.Value = 0;
            Die();
        }
    }

    public void Heal(int amount)
    {
        Assert.IsTrue(amount > 0);

        CurrentHealth.Value += amount;
        if (CurrentHealth.Value > MaxHealth)
        {
            CurrentHealth.Value = MaxHealth;
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} has died.");
        Destroy(gameObject);
    }
}
