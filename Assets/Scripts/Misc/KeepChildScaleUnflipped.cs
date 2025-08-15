using UnityEngine;

public class KeepChildScaleUnflipped : MonoBehaviour
{
    private Vector3 _initialLocalScale;
    private Transform _parentTransform;

    private void Awake()
    {
        _initialLocalScale = transform.localScale;
        _parentTransform = transform.parent;
        if (_parentTransform == null)
            Debug.LogWarning("This object has no parent to track!");
    }

    private void LateUpdate()
    {
        if (_parentTransform == null) return;

        // Invert local scale to counter parent flips
        Vector3 parentScale = _parentTransform.localScale;
        transform.localScale = new Vector3(
            _initialLocalScale.x * Mathf.Sign(parentScale.x),
            _initialLocalScale.y * Mathf.Sign(parentScale.y),
            _initialLocalScale.z * Mathf.Sign(parentScale.z)
        );
    }
}
