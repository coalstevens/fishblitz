public interface IHurtBox
{
    public void TakeDamage(float damage);
    public bool IsVulnerable { get; }
}
