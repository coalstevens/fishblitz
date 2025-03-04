using System.Collections;
using UnityEngine;

public class ReduceOpacityWhenPlayerBehind : MonoBehaviour
{
    [SerializeField] private float fadedOpacity = 0.5f; // The opacity value when the player is behind
    [SerializeField] private float fadeDuration = 0.5f; // The duration of the fade effect

    private SpriteRenderer _spriteRenderer;
    private Color _originalColor;
    private Coroutine _fadeCoroutine;

    private void Start()
    {
        _spriteRenderer = transform.parent.GetComponent<SpriteRenderer>();
        if (_spriteRenderer != null)
        {
            _originalColor = _spriteRenderer.color;
        }
        else
        {
            Debug.LogError("SpriteRenderer component not found on the GameObject.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }
            _fadeCoroutine = StartCoroutine(FadeToOpacity(fadedOpacity));
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
        float startOpacity = _spriteRenderer.color.a;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float newOpacity = Mathf.Lerp(startOpacity, targetOpacity, elapsedTime / fadeDuration);
            _spriteRenderer.color = new Color(_originalColor.r, _originalColor.g, _originalColor.b, newOpacity);
            yield return null;
        }

        _spriteRenderer.color = new Color(_originalColor.r, _originalColor.g, _originalColor.b, targetOpacity);
    }
}
