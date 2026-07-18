using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class ReduceOpacityWhenPlayerBehind : MonoBehaviour
{
    [Tooltip("The opacity value when the player is behind")]
    [SerializeField] private float _fadedOpacity = 0.5f;
    [Tooltip("The duration of the fade effect")]
    [SerializeField] private float _fadeDuration = 0.5f;
    [Tooltip("If true, searches for opacity component on parent. If false, searches on this object.")]
    [SerializeField] private bool _targetParent = true;

    private SpriteRenderer _spriteRenderer;
    private Image _image;
    private CanvasGroup _canvasGroup;
    private SortingGroup _sortingGroup;
    private Color _originalColor;
    private Coroutine _fadeCoroutine;

    private void Start()
    {
        if (GetComponent<Collider2D>() == null)
            Debug.LogWarning("ReduceOpacity: No Collider2D found on this object. OnTriggerEnter2D/Exit2D requires a Collider2D.");

        Transform targetTransform = _targetParent ? transform.parent : transform;

        _spriteRenderer = targetTransform.GetComponent<SpriteRenderer>();
        _image = targetTransform.GetComponent<Image>();
        _canvasGroup = targetTransform.GetComponent<CanvasGroup>();
        _sortingGroup = targetTransform.GetComponent<SortingGroup>();

        int componentCount = 0;
        if (_spriteRenderer != null) componentCount++;
        if (_image != null) componentCount++;
        if (_canvasGroup != null) componentCount++;
        if (_sortingGroup != null) componentCount++;

        if (componentCount == 0)
        {
            Debug.LogError($"ReduceOpacity: No SpriteRenderer, Image, CanvasGroup, or SortingGroup found on {( _targetParent ? "parent" : "this")} object.");
            return;
        }
        else if (componentCount > 1)
        {
            Debug.LogError("ReduceOpacity: Multiple opacity components found - use only one.");
            return;
        }

        if (_spriteRenderer != null)
            _originalColor = _spriteRenderer.color;
        else if (_image != null)
            _originalColor = _image.color;
        else if (_canvasGroup != null)
            _originalColor = new Color(1f, 1f, 1f, _canvasGroup.alpha);
        else if (_sortingGroup != null)
        {
            var renderers = targetTransform.GetComponentsInChildren<SpriteRenderer>(true);
            _originalColor = renderers.Length > 0 ? renderers[0].color : Color.white;
        }
    }

    private void OnDisable()
    {
        if(_fadeCoroutine != null)
            StopCoroutine(_fadeCoroutine);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("ontriggerenter");
        if (other.transform.root.CompareTag("Player"))
        {
            Debug.Log("fade me");
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }
            _fadeCoroutine = StartCoroutine(FadeToOpacity(_fadedOpacity));
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.transform.root.CompareTag("Player"))
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }

            _fadeCoroutine = StartCoroutine(FadeToOpacity(_originalColor.a));
        }
    }

    private IEnumerator FadeToOpacity(float targetOpacity)
    {
        Transform targetTransform = _targetParent ? transform.parent : transform;
        var sortingRenderers = _sortingGroup != null
            ? targetTransform.GetComponentsInChildren<SpriteRenderer>(true)
            : null;
        Color[] sortingOriginals = null;
        if (sortingRenderers != null && sortingRenderers.Length > 0)
        {
            sortingOriginals = new Color[sortingRenderers.Length];
            for (int i = 0; i < sortingRenderers.Length; i++)
                if (sortingRenderers[i] != null)
                    sortingOriginals[i] = sortingRenderers[i].color;
        }

        float startOpacity = _spriteRenderer != null ? _spriteRenderer.color.a :
                             _image != null ? _image.color.a :
                             _canvasGroup != null ? _canvasGroup.alpha :
                             sortingRenderers != null && sortingRenderers.Length > 0 ? sortingRenderers[0].color.a : 1f;
        float elapsedTime = 0f;
        Color targetColor = new Color(_originalColor.r, _originalColor.g, _originalColor.b, targetOpacity);

        while (elapsedTime < _fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float newOpacity = Mathf.Lerp(startOpacity, targetOpacity, elapsedTime / _fadeDuration);
            Color newColor = new Color(_originalColor.r, _originalColor.g, _originalColor.b, newOpacity);

            if (_spriteRenderer != null)
                _spriteRenderer.color = newColor;
            if (_image != null)
                _image.color = newColor;
            if (_canvasGroup != null)
                _canvasGroup.alpha = newOpacity;
            if (sortingRenderers != null)
            {
                for (int i = 0; i < sortingRenderers.Length; i++)
                {
                    if (sortingRenderers[i] == null) continue;
                    Color c = sortingOriginals[i];
                    sortingRenderers[i].color = new Color(c.r, c.g, c.b, newOpacity);
                }
            }

            yield return null;
        }

        if (_spriteRenderer != null)
            _spriteRenderer.color = targetColor;
        if (_image != null)
            _image.color = targetColor;
        if (_canvasGroup != null)
            _canvasGroup.alpha = targetOpacity;
        if (sortingRenderers != null)
        {
            for (int i = 0; i < sortingRenderers.Length; i++)
            {
                if (sortingRenderers[i] == null) continue;
                Color c = sortingOriginals[i];
                sortingRenderers[i].color = new Color(c.r, c.g, c.b, targetOpacity);
            }
        }
    }
}
