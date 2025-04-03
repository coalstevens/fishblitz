using NUnit.Framework;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBasicWeapon", menuName = "Combat/BasicWeapon")]
public class BasicWeapon : ScriptableObject
{
    public float FireRate = 1.0f;
    public float Damage = 1.0f;
    public Sprite ProjectileSprite;
    public float ProjectileLifespan = 8f;
    public float ProjectileSpeed = 5.0f;

    private void Awake()
    {
        Assert.IsNotNull(ProjectileSprite);
    }
}
