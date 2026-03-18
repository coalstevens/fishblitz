using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ReduceOpacityWhenPlayerBehind : MonoBehaviour
{
    [Tooltip("The opacity value when the player is behind")]
    [SerializeField] private float _fadedOpacity = 0.5f;
    [Tooltip("The duration of the fade effect")]
    [SerializeField] private float _fadeDuration = 0.5f;
    [Tooltip("If true, searches for opacity component on parent. If false, searches on this object.")]
    [SerializeField] private bool _searchOnParent = true;

    private SpriteRenderer _spriteRenderer;
    private Image _image;
    private CanvasGroup _canvasGroup;
    private Color _originalColor;
    private Coroutine _fadeCoroutine;

    private void Start()
    {
        Transform targetTransform = _searchOnParent ? transform.parent : transform;

        _spriteRenderer = targetTransform.GetComponent<SpriteRenderer>();
        _image = targetTransform.GetComponent<Image>();
        _canvasGroup = targetTransform.GetComponent<CanvasGroup>();

        int componentCount = 0;
        if (_spriteRenderer != null) componentCount++;
        if (_image != null) componentCount++;
        if (_canvasGroup != null) componentCount++;

        if (componentCount == 0)
        {
            Debug.LogError($"ReduceOpacity: No SpriteRenderer, Image, or CanvasGroup found on {( _searchOnParent ? "parent" : "this")} object.");
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
        else
            _originalColor = new Color(1f, 1f, 1f, _canvasGroup.alpha);
    }

    private void OnDisable()
    {
        if(_fadeCoroutine != null)
            StopCoroutine(_fadeCoroutine);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }
            _fadeCoroutine = StartCoroutine(FadeToOpacity(_fadedOpacity));
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
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
        float startOpacity = _spriteRenderer != null ? _spriteRenderer.color.a :
                             _image != null ? _image.color.a :
                             _canvasGroup.alpha;
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

            yield return null;
        }

        if (_spriteRenderer != null)
            _spriteRenderer.color = targetColor;
        if (_image != null)
            _image.color = targetColor;
        if (_canvasGroup != null)
            _canvasGroup.alpha = targetOpacity;
    }
}
