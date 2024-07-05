using UnityEngine;

public interface IInventoryItem {
    public Sprite ItemSprite {get;set;} 
    public string ItemName {get;}
    public int Quantity {get;set;}
    public int StackCapacity{get;}
}
public interface IPlayerCursorUsingItem {
    public bool UseItemOnWorldObject(IInteractable interactableWorldObject, Vector3Int cursorLocation);
    public bool UseItemOnInteractableTileMap(string tilemapLayerName, Vector3Int cursorLocation);
}
public interface ITool {
    /// <summary>
    /// Uses tool on the world object under player object. Returns false if ignored.
    /// </summary>
    public bool UseToolOnWorldObject(IInteractable interactableWorldObject, Vector3Int cursorLocation);
    
    /// <summary>
    /// Uses tool on the interactive tilemap under cursor. Returns false if ignored.
    /// </summary>
    public bool UseToolOnInteractableTileMap(string tilemapLayerName, Vector3Int cursorLocation);

    /// <summary>
    /// Swings at nothing; plays tool animation.
    /// </summary>
    public void SwingTool();

    /// <summary>
    /// Plays a sound when the tool interacts with the target
    /// </summary>
    public void PlayToolHitSound();
}