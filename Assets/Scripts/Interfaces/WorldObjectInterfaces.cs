using System.Collections.Generic;
using UnityEngine;

public interface ISaveable {
    SaveData Save();
    void Load(SaveData saveData);
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

public interface ITickable {
    public void OnGameMinuteTick();
}