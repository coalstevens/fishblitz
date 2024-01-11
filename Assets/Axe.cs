using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class Axe : MonoBehaviour, ITool, IInventoryItem
{
    private PlayerMovementController _playerMovementController;
    private const string ITEM_HAME = "Axe";
    private int _quantity = 0;
    private const int STACK_CAPACITY = 1;
    public string ItemName { get {return ITEM_HAME;} }
    public int StackCapacity {get {return STACK_CAPACITY;}}
    public Sprite ItemSprite { 
        get => GetComponent<Image>().sprite;
        set => GetComponent<Image>().sprite = value;
    }
    public int Quantity { 
        get => _quantity;
        set => _quantity = value;
    } 

    public void UseTool(TileData tileData, Vector3 cursorLocation)
    {
        Stump _stump = GetStump(cursorLocation);
        if (_stump != null) {
            _stump.SplitLog();
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
