# Crit Charge Damage Implementation

## Changes Needed

### 1. `Assets/Scripts/Combat/ContactHitbox.cs`
Add public damage getter and setter after the `[SerializeField]` fields:

```csharp
public float Damage => _damage;
public void SetDamage(float damage) => _damage = damage;
```

Insert before `private int _friendlyHurtboxLayer;` (line 15).

### 2. `Assets/Scripts/Combat/Projectile.cs`
Three changes:
1. In `Awake()`: add `_spriteRenderer = GetComponent<SpriteRenderer>();` and `Assert.IsNotNull(_spriteRenderer);`, and store `_baseDamage = _hitbox.Damage;`
2. Add `private float _baseDamage;` field, `private bool _isCrit;` field, and `[SerializeField] private SpriteRenderer _spriteRenderer;` — or use a `SpriteRenderer _spriteRenderer` cached in Awake
3. Add method:
```csharp
public void SetCrit(bool value)
{
    _isCrit = value;
    if (value)
    {
        _spriteRenderer.color = Color.yellow;
        _hitbox.SetDamage(_baseDamage * 2);
    }
    else
    {
        _spriteRenderer.color = Color.white;
        _hitbox.SetDamage(_baseDamage);
    }
}
```
4. In `DisableProjectile()`: add `SetCrit(false);` before `ObjectPooling.ReturnObjectToPool(gameObject);`

### 3. `Assets/Scripts/Combat/BowChargeController.cs`
In `Fire()`, after `projectile.Launch(direction, speedMultiplier);` (line 150), add:

```csharp
bool isCrit = _chargeNormalized >= _activeBow.CritShotCharge.x 
           && _chargeNormalized < _activeBow.CritShotCharge.y;
if (isCrit) projectile.SetCrit(true);
```

## Verification
- The crit range check matches `UpdateOverrideColor()` logic
- Pooled projectiles are reset to non-crit state via `DisableProjectile()`
- No prefab changes needed (SpriteRenderer is on the same GameObject as Projectile)
