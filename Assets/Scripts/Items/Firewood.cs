using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "NewFirewood", menuName = "Items/Firewood")]
public class Firewood : Inventory.Item //IPlayerCursorUsingItem
{
    public void CursorAction(TileData tileData, Vector3 cursorLocation)
    {
        LarchStump _stump = GetStump(cursorLocation);
        if (_stump != null) {
            _stump.LoadLog();
        }
    }

    // TODO Convert to GetFirePlace or GetWoodRack 
    private LarchStump GetStump(Vector3 cursorLocation)
    {
        List<Collider2D> _results = new List<Collider2D>();
        Physics2D.OverlapBox(cursorLocation + new Vector3(0.5f, 0.5f, 0f), new Vector2(1, 1), 0, new ContactFilter2D().NoFilter(), _results);

        foreach (var _result in _results)
        {
            var _placedItem = _result.GetComponent<LarchStump>();
            if (_placedItem != null)
            {
                return _placedItem;
            }
        }
        return null;
    }
}
