using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

public class StaticSpriteSorting : MonoBehaviour
{
    private void OnEnable()
    {
        SortSprite();
    }

    public void SortSprite()
    {
        SpriteRenderer _spriteRenderer = GetComponent<SpriteRenderer>();
        SortingGroup _sortingGroup = GetComponent<SortingGroup>();
        Assert.IsTrue(_spriteRenderer != null || _sortingGroup != null);

        if (_spriteRenderer != null)
            _spriteRenderer.sortingOrder = Mathf.RoundToInt(transform.position.y * 100f);
        else
            _sortingGroup.sortingOrder = Mathf.RoundToInt(transform.position.y * 100f);
    }
}
