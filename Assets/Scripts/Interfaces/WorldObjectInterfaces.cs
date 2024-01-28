using System.Collections.Generic;
using UnityEngine;

public interface IWorldObject {
    public string Identifier{get;}
    public int State{get;set;}
}
public interface IInteractable {
    /// <summary>
    /// Returns false if the object ignores the command.
    /// </summary>
    public bool CursorInteract(Vector3 cursorLocation);
    public Collider2D InteractCollider{get;}
}
public interface IInteractableTile {
    /// <summary>
    /// Returns false if the tile ignores the command.
    /// </summary>
}
public interface IItemStorage {
    public Dictionary<string, int> ItemQuantities{get;set;}
}