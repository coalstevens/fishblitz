using System;
using System.Collections;
using DG.Tweening.Plugins.Options;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class BirdMaterialController : MonoBehaviour
{
    public enum OutlineColor { Base, Highlight, Mask }
    [SerializeField] private Vector2 _pivotOffsetForWipePixels = Vector2.zero;
    [SerializeField] private Shader _outlineShader;
    [SerializeField] private Color _baseColor = Color.white;
    [SerializeField] private Color _highlightColor = Color.black;
    [SerializeField, UnityEngine.Range(0f, 6.2831853f)] private float _startAngle = 0f;
    [SerializeField] private float _wipeDuration = 1f;
    [Header("On Tag")]
    [SerializeField] private float _holdHighlightDuration = 2f;
    private OutlineColor _currentState = OutlineColor.Base;
    private Material _mat;
    private SpriteRenderer _renderer;
    private Coroutine _wipeCoroutine;
    private Action _unsubscribe;
    private BirdBrain _bird;
    private bool _isStateLocked = false;

    private void Start()
    {
        Assert.IsNotNull(_outlineShader, "Outline shader is not assigned in the Inspector.");
        _renderer = GetComponent<SpriteRenderer>();
        _bird = GetComponentInParent<BirdBrain>();
        _mat = _renderer.material;

        Assert.IsTrue(_mat.shader == _outlineShader, "Material shader does not match the outline shader.");
        Assert.IsTrue(_mat.HasProperty("_Color1"), "_Color1 property is missing from the material.");
        Assert.IsTrue(_mat.HasProperty("_Color2"), "_Color2 property is missing from the material.");
        Assert.IsTrue(_mat.HasProperty("_Color1IsMask"), "_Color1IsMask property is missing from the material.");
        Assert.IsTrue(_mat.HasProperty("_Color2IsMask"), "_Color2IsMask property is missing from the material.");
        Assert.IsTrue(_mat.HasProperty("_AngleRange"), "_AngleRange property is missing from the material.");

        _mat.SetVector("_PivotOffset_Pixels", new Vector4(_pivotOffsetForWipePixels.x, _pivotOffsetForWipePixels.y, 0f, 0f));
        TryChangeOutlineColor(OutlineColor.Mask, false);
        // WipeToState(OutlineColor.Highlight);
        _unsubscribe = _bird.InstanceData.IsTagged.OnChange((curr) => OnBirdTagged(curr));
    }

    private void LateUpdate()
    {
        Sprite sprite = _renderer.sprite;
        Vector2 pivotUV = new Vector2(
            (sprite.textureRect.x + sprite.pivot.x + _pivotOffsetForWipePixels.x) / sprite.texture.width,
            (sprite.textureRect.y + sprite.pivot.y + _pivotOffsetForWipePixels.y) / sprite.texture.height
        );
        _mat.SetVector("_PivotPositionUV", pivotUV);
    }

    private void OnDisable()
    {
        _unsubscribe?.Invoke();
        _unsubscribe = null;
    }

    private void OnBirdTagged(bool curr)
    {
        Debug.Log("OnBirdTagged called");
        if (curr)
        {
            StartCoroutine(TagCoroutine());
        }
    }

    private IEnumerator TagCoroutine()
    {
        SetOutlineState(OutlineColor.Highlight);
        _isStateLocked = true;
        yield return new WaitForSeconds(_holdHighlightDuration);
        WipeToState(OutlineColor.Mask);
        yield return new WaitForSeconds(_wipeDuration);
        _isStateLocked = false;
    }

    /// <summary>
    /// Returns false if state was locked and outline was not changed.
    /// </summary>
    public bool TryChangeOutlineColor(OutlineColor color, bool useWipe, float lockDuration = 0f)
    {
        if (_isStateLocked)
            return false;
        if (lockDuration > 0f)
        {
            _isStateLocked = true;
            Invoke(nameof(UnlockState), lockDuration);
        }
        if (useWipe)
            WipeToState(color);
        else
            SetOutlineState(color);
        return true;
    }

    private void UnlockState()
    {
        _isStateLocked = false;
    }

    private void SetOutlineState(OutlineColor state)
    {
        Debug.Log("Set outline called");
        _currentState = state;
        SetOutlineToColor1(state);
    }

    private void WipeToState(OutlineColor targetState)
    {
        Debug.Log("Wipe called");
        if (_wipeCoroutine != null)
        {
            StopCoroutine(_wipeCoroutine);
            _wipeCoroutine = null;
        }
        _wipeCoroutine = StartCoroutine(WipeCoroutine(targetState));
    }

    private IEnumerator WipeCoroutine(OutlineColor targetState)
    {
        // Set Wipe Target Color
        switch (targetState)
        {
            case OutlineColor.Base:
                _mat.SetColor("_Color2", _baseColor);
                _mat.SetInt("_Color2IsMask", 0);
                break;
            case OutlineColor.Highlight:
                _mat.SetColor("_Color2", _highlightColor);
                _mat.SetInt("_Color2IsMask", 0);
                break;
            case OutlineColor.Mask:
                _mat.SetInt("_Color2IsMask", 1);
                break;
        }

        // Perform Wipe
        float elapsed = 0f;

        while (elapsed < _wipeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _wipeDuration);
            float endAngle = _startAngle + Mathf.Lerp(0f, Mathf.PI * 2f, t);
            // Debug.Log($"Wipe Progress {endAngle}");
            _mat.SetVector("_AngleRange", new Vector4(_startAngle, endAngle, 0f, 0f));

            yield return null;
        }

        _currentState = targetState;
        SetOutlineToColor1(_currentState);
    }

    private void SetOutlineToColor1(OutlineColor state)
    {
        if (_wipeCoroutine != null)
        {
            StopCoroutine(_wipeCoroutine);
            _wipeCoroutine = null;
        }

        switch (state)
        {
            case OutlineColor.Base:
                _mat.SetColor("_Color1", _baseColor);
                _mat.SetInt("_Color1IsMask", 0);
                break;
            case OutlineColor.Highlight:
                _mat.SetColor("_Color1", _highlightColor);
                _mat.SetInt("_Color1IsMask", 0);
                break;
            case OutlineColor.Mask:
                _mat.SetInt("_Color1IsMask", 1);
                break;
        }

        _mat.SetVector("_AngleRange", new Vector4(0, Mathf.PI * 2f, 0f, 0f));
    }
}
