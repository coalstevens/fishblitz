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
            DisableProgressBar();
        }
        else
        {
            _unsubscribeReload?.Invoke();
            _unsubscribeReload = curr.IsReloading.OnChange(curr => HandleProgressBar(curr));
            HandleProgressBar(curr.IsReloading.Value);
        }
    }

    private void HandleProgressBar(bool isReloading)
    {
        if (isReloading)
        {
            EnableProgressBar();
        }
        else
        {
            DisableProgressBar();
        }
    }

    private void EnableProgressBar()
    {
        _progressBar.enabled = true;
        _progressBar.color = _reloadBarColor;
        if (_reloadCoroutine != null) {
            Debug.LogError("Reload coroutine already running, stopping it before starting a new one.");
            StopCoroutine(_reloadCoroutine);
        }
        _reloadCoroutine = StartCoroutine(UpdateProgressBar());
    }

    private void DisableProgressBar()
    {
        if (_reloadCoroutine != null)
            StopCoroutine(_reloadCoroutine);
        _progressBar.enabled = false;
    }

    private IEnumerator UpdateProgressBar()
    {
        while (_reloader.ActiveWeaponData.Value.IsReloading.Value)
        {
            float newWidth = Mathf.Lerp(0, 1, _reloader.ActiveWeaponData.Value.ReloadElapsed / _reloader.ActiveWeapon.Value.ReloadTime);
            _progressBar.transform.localScale = new Vector3(newWidth, 1, 1);
            yield return null;
        }
    }
}
