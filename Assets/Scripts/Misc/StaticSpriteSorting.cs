using UnityEngine;

public class StaticSpriteSorting : MonoBehaviour
{
    private void Start()
    {
        int sortingOrder = Mathf.RoundToInt(transform.position.y * 100f);
        GetComponent<SpriteRenderer>().sortingOrder = -sortingOrder;
    }
}
