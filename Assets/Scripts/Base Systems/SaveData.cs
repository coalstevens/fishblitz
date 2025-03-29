using System;
using Newtonsoft.Json;
using UnityEngine;


public class SaveData {

    public interface ISaveable 
    {
        SaveData Save();
        void Load(SaveData saveData);
    }

    public string Identifier;
    public SimpleVector3 Position;
    public string ExtendedData;

    // SimpleVector3 exists because you can't json serialize a Vector3, not sure why. This works.
    public class SimpleVector3 {
        public float x;
        public float y;
        public float z;
    
        public SimpleVector3(Vector3 _vector) {
            x = _vector.x;
            y = _vector.y;
            z = _vector.z;
        }
    }

    public bool AddIdentifier(string identifier) {
        if (Resources.Load<GameObject>("WorldObjects/" + identifier) == null) {
            Debug.LogError($"Prefab not found for identifier: {identifier}");
            return false;
        }
        Identifier = identifier;
        return true;
    }
        
    public void AddTransformPosition(Vector3 position) {
        Position = new SimpleVector3(position);
    }

    public bool AddExtendedSaveData<T>(T data) {
        try {
            ExtendedData = JsonConvert.SerializeObject(data);
            return true;
        }
        catch (Exception ex) {
            Debug.Log($"Error converting object to JSON: {ex.Message}");
            return false;
        }
    }

    public T GetExtendedSaveData<T>()
    {
        try {
            return JsonConvert.DeserializeObject<T>(ExtendedData);
        }
        catch (Exception ex) {
            Console.WriteLine($"Error converting JSON to object: {ex.Message}");
            return default(T); // Return default value if deserialization fails
        }
    }

    public GameObject InstantiateGameObjectFromSaveData(Transform parent) {
        if (Identifier == null) {
            Debug.LogError("There is no identifier to load the Worldobject");
            return null;
        }

        GameObject _prefab = Resources.Load<GameObject>("WorldObjects/" + Identifier);
        if (parent == null) { 
            Debug.LogError("The parent gameobject doesn't exist.");
            return null;
        }

        Vector3 _savedPosition = Position == null ? Vector3.zero : new Vector3(Position.x, Position.y, Position.z); 
        GameObject _newObject = UnityEngine.Object.Instantiate(_prefab, _savedPosition, Quaternion.identity, parent);

        return _newObject;
    }
}
