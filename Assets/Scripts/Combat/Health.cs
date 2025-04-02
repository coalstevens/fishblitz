using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 2f;

    [Header("Regeneration")]
    [SerializeField] private bool regenerateHealth = false; 
    [SerializeField] private float regenerateInterval = 1f; // Heals 1 every interval
    private float currentHealth;
    public float CurrentHealth => currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} has died.");
        // Add death logic here (e.g., destroy object, trigger animation, etc.)
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        IDamaging damaging = collision.collider.GetComponent<IDamaging>();
        if (damaging != null)
        {
            TakeDamage(damaging.DamageAmount);
        }
    }
}
