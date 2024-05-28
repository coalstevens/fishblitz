using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInventoryItem {
    public Sprite ItemSprite {get;set;} 
    public string ItemName {get;}
    public int Quantity {get;set;}
    public int StackCapacity{get;}
}
public interface IPlayerCursorUsingItem {
    public void UseItemOnWorldObject(IInteractable interactableWorldObject, Vector3Int cursorLocation);
    public void UseItemOnTile(IInteractableTile interactableTile, Vector3Int cursorLocation);
}
public interface ITool {
    public void UseToolOnWorldObject(IInteractable interactableWorldObject, Vector3Int cursorLocation);
    public void UseToolOnTile(IInteractableTile interactableTile, Vector3Int cursorLocation);
}