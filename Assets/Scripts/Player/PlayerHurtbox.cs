using UnityEngine;
using UnityEngine.Assertions;

public class PlayerHurtbox : MonoBehaviour, IHurtBox
{
    private PlayerHealth _playerHealth;

    public bool IsVulnerable => !_playerHealth.IsInvulnerable.Value;

    private void Awake()
    {
        _playerHealth = GetComponentInParent<PlayerHealth>();
        Assert.IsNotNull(_playerHealth);
    }

    public void TakeDamage(float damage)
    {
        _playerHealth.TakeDamage(damage);
    }
}

