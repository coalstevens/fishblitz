using UnityEngine;
using UnityEngine.Assertions;

public class EnemyHurtbox : MonoBehaviour, IHurtBox
{
    private EnemyHealth _combatStatus;

    public bool IsVulnerable => !_combatStatus.IsInvulnerable.Value;

    private void OnEnable()
    {
        _combatStatus = GetComponentInParent<EnemyHealth>();
        Assert.IsNotNull(_combatStatus);
    }

    public void TakeDamage(float damage)
    {
        _combatStatus.TakeDamage(damage);
    }
}
