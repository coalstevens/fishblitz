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
        Assert.IsTrue(_spriteRenderer != null || _sortingGroup != null, "There is no sprite renderer or sorting group attached to this gameobject");
    }

    private void Update()
    {
        int sortingOrder = Mathf.RoundToInt(transform.position.y * 100f);
        if (_spriteRenderer != null)
            _spriteRenderer.sortingOrder = -sortingOrder;
        else if (_sortingGroup != null)
            _sortingGroup.sortingOrder = -sortingOrder;
        else 
            Debug.LogError("There is no sprite renderer or sorting group attached to this gameobject");
    }
}