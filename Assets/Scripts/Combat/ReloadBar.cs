using UnityEngine;
using UnityEngine.Assertions;

public class ReloadBar : MonoBehaviour
{
    [SerializeField] private Color _reloadBarColor;
    private IWeapon _weapon;
    private SpriteRenderer _progressBar;
    private bool _isReloading = false;

    private void Awake()
    {
        _weapon = transform.parent.GetComponentInChildren<IWeapon>();
        _progressBar = transform.GetChild(0).GetComponent<SpriteRenderer>();
        Assert.IsNotNull(_weapon);
        Assert.IsNotNull(_progressBar);
    }

    private void OnEnable()
    {
        _weapon.OnReloadStart += HandleReloadStart;
        _weapon.OnReloadComplete += HandleReloadComplete;
        _progressBar.color = _reloadBarColor;
    }


    private void OnDisable()
    {
        _weapon.OnReloadStart -= HandleReloadStart;
        _weapon.OnReloadComplete -= HandleReloadComplete;
    }

    private void HandleReloadComplete()
    {
        _isReloading = false;
        _progressBar.enabled = false;
    }

    private void HandleReloadStart()
    {
        _isReloading = true;
        _progressBar.enabled = true;
    }

    private void Update()
    {
        if (_isReloading)
        {
            UpdateProgressMeter();
        }
    }

    private void UpdateProgressMeter()
    {
        float newWidth = Mathf.Lerp(0, 1, _weapon.ReloadElapsedSecs / _weapon.ReloadTimeSecs);
        _progressBar.transform.localScale = new Vector3(newWidth, 1, 1);
    }
}
