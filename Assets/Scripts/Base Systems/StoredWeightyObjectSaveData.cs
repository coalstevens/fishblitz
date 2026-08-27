using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

public class StoredWeightyObjectSaveData
{
    public string PrefabId;
    public string PersistentID;
    public float PositionX;
    public float PositionY;
    public float PositionZ;
    public string StateJson;
    public string TypeName;

    public static StoredWeightyObjectSaveData Capture(StoredWeightyObject storedObject)
    {
        var _record = storedObject.Record;
        return new StoredWeightyObjectSaveData
        {
            PrefabId = _record.PrefabId,
            PersistentID = _record.PersistentID,
            PositionX = _record.Position.x,
            PositionY = _record.Position.y,
            PositionZ = _record.Position.z,
            StateJson = _record.StateJson,
            TypeName = storedObject.Type != null ? storedObject.Type.name : null
        };
    }

    public static List<StoredWeightyObjectSaveData> CaptureAll(IEnumerable<StoredWeightyObject> storedObjects)
    {
        var _list = new List<StoredWeightyObjectSaveData>();
        foreach (var _storedObject in storedObjects)
            _list.Add(Capture(_storedObject));
        return _list;
    }

    public StoredWeightyObject Restore()
    {
        var _type = Resources.Load<WeightyObjectType>("WeightyObjects/" + TypeName);
        if (_type == null)
        {
            Debug.LogError($"WeightyObjectType not found: {TypeName}");
            return null;
        }

        var _record = new SceneObjectRecord
        {
            PrefabId = PrefabId,
            PersistentID = PersistentID,
            Position = new Vector3(PositionX, PositionY, PositionZ),
            StateJson = StateJson
        };

        return new StoredWeightyObject(_type, _record);
    }
}

public class StackContentsSaveData
{
    public bool Holding;
    public List<StoredWeightyObjectSaveData> Items = new();
}
