using UnityEngine;

public class SpawnOffset : MonoBehaviour
{
    [SerializeField] private Vector2 _offset;
    public Vector2 Offset => _offset;

    private void OnValidate()
    {
        if (Mathf.Abs(_offset.x) > 1f || Mathf.Abs(_offset.y) > 1f)
            Debug.LogWarning("SpawnOffset: Are you sure? Offset values > 1 may be in pixels instead of world space. Remember: 1 unit = 1 tile.");
    }
}
