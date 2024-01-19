using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class Log : MonoBehaviour, IInventoryItem, IPlayerCursorUsingItem
{
    private const string ITEM_HAME = "Log";
    private int _quantity = 0;
    private const int STACK_CAPACITY = 99;
    public int StackCapacity {get {return STACK_CAPACITY;}}
    public string ItemName { get {return ITEM_HAME;} }
    public Sprite ItemSprite { 
        get => GetComponent<Image>().sprite;
        set => GetComponent<Image>().sprite = value;
    }
    public int Quantity { 
        get => _quantity;
        set => _quantity = value;
    }

    public void CursorAction(TileData tileData, Vector3 cursorLocation)
    {
        Stump _stump = GetStump(cursorLocation);
        if (_stump != null) {
            _stump.LoadLog();
        }
    }
    
    private Stump GetStump(Vector3 cursorLocation)
    {
        List<Collider2D> _results = new List<Collider2D>();
        Physics2D.OverlapBox(cursorLocation + new Vector3(0.5f, 0.5f, 0f), new Vector2(1, 1), 0, new ContactFilter2D().NoFilter(), _results);

        foreach (var _result in _results)
        {
            var _placedItem = _result.GetComponent<Stump>();
            if (_placedItem != null)
            {
                return _placedItem;
            }
        }
        return null;
    }
}
