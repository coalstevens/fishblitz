using System;
using UnityEngine;

[Serializable]
public class SceneObjectRecord
{
    public string PrefabId;
    public string PersistentID;
    public Vector3 Position;
    public string StateJson;

    public static SceneObjectRecord Capture(ISceneSaveable saveable, Vector3 position)
    {
        if (string.IsNullOrEmpty(saveable.PersistentID))
            saveable.PersistentID = Guid.NewGuid().ToString();

        return new SceneObjectRecord
        {
            PrefabId = saveable.PrefabId,
            PersistentID = saveable.PersistentID,
            Position = position,
            StateJson = saveable.CaptureState()
        };
    }

    public void Restore(ISceneSaveable saveable)
    {
        saveable.PersistentID = PersistentID;
        if (!string.IsNullOrEmpty(StateJson))
            saveable.RestoreState(StateJson);
    }

    public GameObject Instantiate(Transform parent)
    {
        if (string.IsNullOrEmpty(PrefabId))
        {
            Debug.LogError("There is no identifier to load the WorldObject");
            return null;
        }

        GameObject prefab = Resources.Load<GameObject>("WorldObjects/" + PrefabId);
        if (prefab == null)
        {
            Debug.LogError($"Prefab not found for identifier: {PrefabId}");
            return null;
        }

        if (parent == null)
        {
            Debug.LogError("The parent gameobject doesn't exist.");
            return null;
        }

        return UnityEngine.Object.Instantiate(prefab, Position, Quaternion.identity, parent);
    }
}
