using UnityEngine;

public class AnimateTitle : MonoBehaviour
{
    [SerializeField] private float _scaleFactor = 1;
    [SerializeField] private float _speed = 5f;

    private Vector2 _originalScale;

    private void Awake()
    {
        _originalScale = transform.localScale;
    }

    void Update()
    {
        var sinVal = Mathf.Sin(Time.time * _speed);
        var scaleVal = sinVal * _scaleFactor;

        transform.localScale = _originalScale + Vector2.one * scaleVal;
    }
}
