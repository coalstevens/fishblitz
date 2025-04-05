using ReactiveUnity;
using UnityEngine;

public class Reloader : MonoBehaviour
{
    [HideInInspector] public Reactive<RangedWeaponItem> ActiveWeapon = new Reactive<RangedWeaponItem>(null);
    [HideInInspector] public Reactive<RangedWeaponItem.InstanceData> ActiveWeaponData = new Reactive<RangedWeaponItem.InstanceData>(null);
    public float CoolDownTime
    {
        get
        {
            if (ActiveWeapon.Value == null)
                return 0f;
            return 1f / ActiveWeapon.Value.FireRateShotsPerSec;
        }
    }

    public void SetActiveWeapon(RangedWeaponItem weapon, RangedWeaponItem.InstanceData weaponData)
    {
        ActiveWeapon.Value = weapon;
        ActiveWeaponData.Value = weaponData;
    }

    private void FixedUpdate()
    {
        if (ActiveWeapon == null || ActiveWeaponData == null)
            return;

        if (!ActiveWeaponData.Value.IsReloading.Value && !ActiveWeaponData.Value.IsCoolingDown.Value)
            return;

        if (ActiveWeaponData.Value.IsReloading.Value)
            UpdateReloading();

        if (ActiveWeaponData.Value.IsCoolingDown.Value)
            UpdateCooldown();
    }

    private void UpdateReloading()
    {
        ActiveWeaponData.Value.ReloadElapsed += Time.fixedDeltaTime;
        if (ActiveWeaponData.Value.ReloadElapsed >= ActiveWeapon.Value.ReloadTime)
        {
            ActiveWeaponData.Value.ReloadElapsed = 0;
            ActiveWeaponData.Value.IsReloading.Value = false;
            ActiveWeaponData.Value.CurrentClipCount = ActiveWeapon.Value.ClipSize;
        }
    }

    private void UpdateCooldown()
    {
        ActiveWeaponData.Value.CoolDownElapsed += Time.fixedDeltaTime;
        if (ActiveWeaponData.Value.CoolDownElapsed >= CoolDownTime)
        {
            ActiveWeaponData.Value.CoolDownElapsed = 0;
            ActiveWeaponData.Value.IsCoolingDown.Value = false;
        }
    }
}
