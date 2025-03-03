using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicSpriteSorting : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;

    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        // Calculate sorting order based on Y position
        int sortingOrder = Mathf.RoundToInt(transform.position.y * 100f);
        _spriteRenderer.sortingOrder = -sortingOrder;
    }
}