using System.Collections.Generic;
using UnityEngine;

public interface ISaveable<T> where T : WorldObjectSaveData
{
    T Save();
    void Load(T saveData);
}

//TODO define me plox
public interface IInteractableTile {
}

public interface IInteractable {
    /// <summary>
    /// Returns false if the object ignores the command.
    /// </summary>
    public bool CursorInteract(Vector3 cursorLocation);
}

public interface IHeatSensitive {
    public HeatSensitiveManager HeatSensitive{get;}
}

public interface IHeatSource {
    public HeatSourceManager HeatSource {get;}
}

public interface ITickable {
    public void OnGameMinuteTick();
}