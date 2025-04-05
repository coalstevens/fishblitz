using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

public class ReloadBar : MonoBehaviour
{
    [SerializeField] private Color _reloadBarColor;
    [SerializeField] private Reloader _reloader;
    private SpriteRenderer _progressBar;
    private Action _unsubscribeWeapon;
    private Action _unsubscribeReload;
    private Coroutine _reloadCoroutine;

    private void Awake()
    {
        _progressBar = transform.GetChild(0).GetComponent<SpriteRenderer>();
        Assert.IsNotNull(_reloader);
        Assert.IsNotNull(_progressBar);
    }

    private void OnEnable()
    {
        _unsubscribeWeapon = _reloader.ActiveWeaponData.OnChange(curr => SubscribeToActiveWeaponReload(curr));
        _progressBar.color = _reloadBarColor;
    }

    private void OnDisable()
    {
        _unsubscribeWeapon?.Invoke();
        _unsubscribeWeapon = null;
    }

    private void SubscribeToActiveWeaponReload(RangedWeaponItem.InstanceData curr) 
    { 
        if (curr == null)
        {
            _unsubscribeReload?.Invoke();
            _unsubscribeReload = null;
        }
        else
        {
            _unsubscribeReload?.Invoke();
            _unsubscribeReload = curr.IsReloading.OnChange(curr => HandleReload(curr));
            HandleReload(curr.IsReloading.Value);
        }
    }

    private void HandleReload(bool isReloading)
    {
        if (isReloading)
        {
            _progressBar.enabled = true;
            _progressBar.color = _reloadBarColor;
            _reloadCoroutine = StartCoroutine(UpdateProgressMeter());
        }
        else
        {
            if (_reloadCoroutine != null)
                StopCoroutine(_reloadCoroutine);
            _progressBar.enabled = false;
        }
    }

    private IEnumerator UpdateProgressMeter()
    {
        while (_reloader.ActiveWeaponData.Value.IsReloading.Value)
        {
            float newWidth = Mathf.Lerp(0, 1, _reloader.ActiveWeaponData.Value.ReloadElapsed / _reloader.ActiveWeapon.Value.ReloadTime);
            _progressBar.transform.localScale = new Vector3(newWidth, 1, 1);
            yield return null;
        }
    }
}
