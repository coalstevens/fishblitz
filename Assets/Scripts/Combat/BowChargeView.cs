using UnityEngine;
using UnityEngine.InputSystem;

public class BowChargeView : MonoBehaviour
{
    [SerializeField] private Transform _frame;
    [SerializeField] private Transform _HUD;
    [SerializeField] private Transform _rotationPivot;
    [SerializeField] private Vector2 _framePositionLimits = new Vector2(0f, 1f);

    private SpriteRenderer[] _frameRenderers;
    private static readonly int _overrideColorProp = Shader.PropertyToID("_OverrideColor");
    private static readonly int _overridePercentProp = Shader.PropertyToID("_OverridePercent");

    private void Awake()
    {
        _frameRenderers = _frame.GetComponentsInChildren<SpriteRenderer>();
        _HUD.gameObject.SetActive(false);
    }

    public void ShowCharge()
    {
        _frame.localPosition = new Vector3(_framePositionLimits.x, 0f, 0f);
        SetFrameAlpha(0f);
        _HUD.gameObject.SetActive(true);
    }

    public void HideCharge()
    {
        _HUD.gameObject.SetActive(false);
        _frame.localPosition = new Vector3(_framePositionLimits.x, 0f, 0f);
        SetFrameAlpha(0f);
        ClearOverride();
        if (_rotationPivot != null)
            _rotationPivot.localRotation = Quaternion.identity;
    }

    public void SetChargeNormalized(float t)
    {
        float x = Mathf.Lerp(_framePositionLimits.x, _framePositionLimits.y, t);
        _frame.localPosition = new Vector3(x, 0f, 0f);
    }

    public void UpdateChargeAlpha(float chargeNormalized, float minCharge)
    {
        float t = Mathf.Clamp01(chargeNormalized / minCharge);
        SetFrameAlpha(t * t);
    }

    public void UpdateCritVisual(float chargeNormalized, Vector2 critShot, float minCharge)
    {
        if (chargeNormalized < critShot.x)
        {
            float t = Mathf.InverseLerp(minCharge, critShot.x, chargeNormalized);
            SetOverride(Color.white, t);
        }
        else if (chargeNormalized < critShot.y)
        {
            SetOverride(Color.yellow, 1f);
        }
        else
        {
            ClearOverride();
        }
    }

    public void AlignPivotToMouse()
    {
        if (_rotationPivot == null) return;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 dir = mouseWorld - _rotationPivot.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        _rotationPivot.localEulerAngles = new Vector3(0f, 0f, angle);
    }

    private void SetFrameAlpha(float alpha)
    {
        foreach (var sr in _frameRenderers)
        {
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
    }

    private void SetOverride(Color color, float value)
    {
        var block = new MaterialPropertyBlock();
        block.SetColor(_overrideColorProp, color);
        block.SetFloat(_overridePercentProp, value);
        foreach (var sr in _frameRenderers)
        {
            sr.SetPropertyBlock(block);
        }
    }

    private void ClearOverride()
    {
        foreach (var sr in _frameRenderers)
        {
            sr.SetPropertyBlock(null);
        }
    }
}
