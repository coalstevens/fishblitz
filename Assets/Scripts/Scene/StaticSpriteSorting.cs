using UnityEngine;

public class StaticSpriteSorting : MonoBehaviour
{
    private void OnEnable()
    {
        SortSprite();
    }

    public void SortSprite()
    {
        int sortingOrder = Mathf.RoundToInt(transform.position.y * 100f);
        GetComponent<SpriteRenderer>().sortingOrder = -sortingOrder;
    }
}
