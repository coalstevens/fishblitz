using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

public class DynamicSpriteSorting : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private SortingGroup _sortingGroup;

    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _sortingGroup = GetComponent<SortingGroup>();
    }

    private void Update()
    {
        Assert.IsTrue(_spriteRenderer != null || _sortingGroup != null, "There is no sprite renderer or sorting group attached to this gameobject");
        if (_spriteRenderer != null)
            _spriteRenderer.sortingOrder = -Mathf.RoundToInt(transform.position.y * 100f);
        else 
            _sortingGroup.sortingOrder = -Mathf.RoundToInt(transform.position.y * 100f);
    }
}