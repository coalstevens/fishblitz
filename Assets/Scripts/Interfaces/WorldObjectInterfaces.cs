using System.Collections.Generic;
using UnityEngine;

//TODO define me plox
public interface IInteractableTile {
}

public interface IWorldObject {
    public string Identifier{get;}
    public int State{get;set;}
    public Collider2D ObjCollider{get;}
}
public interface IInteractable {
    /// <summary>
    /// Returns false if the object ignores the command.
    /// </summary>
    public bool CursorInteract(Vector3 cursorLocation);
}
public interface IItemStorage : IWorldObject {
    public Dictionary<string, int> ItemQuantities{get;set;}
}

public interface IHeatSensitive : IWorldObject {
    public HeatSensitiveManager HeatSensitive{get;}
}

public interface IHeatSource : IWorldObject {
    public HeatSourceManager HeatSource {get;}
}

public interface ITimeSensitive : IWorldObject {
    public List<float> CountersGameMinutes{get;set;}
    public void OnGameMinuteTick();
}