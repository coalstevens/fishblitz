using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class ItemCursor : MonoBehaviour
{
    [SerializeField] private Inventory _inventory;
    [SerializeField] private Transform _itemSlotContainer;
    [Header("Label")]
    [SerializeField] private RectTransform _fill;
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private float _sidePadding;
    [SerializeField] private float _fadeAfterDurationSecs = 2f;
    [SerializeField] private float _fadeDurationSecs = 2f;
    private Action _unsubscribe;
    private Coroutine _labelFadeRoutine;

    private void OnEnable()
    {
        _unsubscribe = _inventory.ActiveItemSlot.OnChange(curr => OnActiveItemChange(curr));
        StartCoroutine(WaitThenUpdate());
    }

    private void OnDisable()
    {
        _unsubscribe();
    }

    private void OnActiveItemChange(int newSlotNum)
    {
        transform.position = _itemSlotContainer.GetChild(newSlotNum).position;
        UpdateLabel(newSlotNum);
    }

    private IEnumerator WaitThenUpdate()
    {
        yield return null;
        OnActiveItemChange(_inventory.ActiveItemSlot.Value);
    }

    private void UpdateLabel(int newSlotNum)
    {
        if (_labelFadeRoutine != null)
            StopCoroutine(_labelFadeRoutine);

        if (TrySetLabelText(newSlotNum))
        {
            ResizeLabel();
            SetLabelOpacity(1);
            _labelFadeRoutine = StartCoroutine(FadeOutLabel());
        }
        else
        {
            SetLabelOpacity(0);
        }
    }

    private bool TrySetLabelText(int newSlotNum)
    {
        var _itemType = _inventory.GetActiveItem();
        if (_itemType == null || string.IsNullOrEmpty(_itemType.ItemLabel))
            return false;

        _text.text = _itemType.ItemLabel;
        return true;
    }

    private void ResizeLabel()
    {
        _text.ForceMeshUpdate(); // required??
        float _preferredWidth = _text.preferredWidth;
        _text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _preferredWidth);
        _fill.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _preferredWidth + _sidePadding * 2);
    }

    private void SetLabelOpacity(float opacity)
    {
        CanvasGroup _canvasGroup = _fill.GetComponent<CanvasGroup>();
        _canvasGroup.alpha = opacity;
    }

    private IEnumerator FadeOutLabel()
    {
        yield return new WaitForSeconds(_fadeAfterDurationSecs);

        CanvasGroup _canvasGroup = _fill.GetComponent<CanvasGroup>();
        float _startAlpha = _canvasGroup.alpha;
        float _time = 0;

        while (_time < _fadeDurationSecs)
        {
            _time += Time.deltaTime;
            _canvasGroup.alpha = Mathf.Lerp(_startAlpha, 0, _time / _fadeDurationSecs);
            yield return null;
        }

        _canvasGroup.alpha = 0;
    }
}
