using System;
using Newtonsoft.Json;
using UnityEngine;

public class SaveData {
    public string _identifier;
    public SimpleVector3 _position;
    public string _extendedData;

    // Exists because you can't json serialize a Vector3 
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
        _identifier = identifier;
        return true;
    }
        
    public void AddTransformPosition(Vector3 position) {
        _position = new SimpleVector3(position);
    }

    public bool AddExtendedSaveData<T>(T data) {
        // THIS IS WHERE YOU WERE
        // _EXTENDEDDATA IS NOT SAVING TO JSON, SEE TEXTEDIT
        try {
            _extendedData = JsonConvert.SerializeObject(data);
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
            return JsonConvert.DeserializeObject<T>(_extendedData);
        }
        catch (Exception ex) {
            Console.WriteLine($"Error converting JSON to object: {ex.Message}");
            return default(T); // Return default value if deserialization fails
        }
    }

    public GameObject InstantiateGameObjectFromSaveData(Transform parent) {
        if (_identifier == null) {
            Debug.LogError("There is no identifier to load the Worldobject");
            return null;
        }

        GameObject _prefab = Resources.Load<GameObject>("WorldObjects/" + _identifier);
        if (parent == null) { 
            Debug.LogError("The parent gameobject doesn't exist.");
            return null;
        }

        Vector3 _savedPosition = _position == null ? Vector3.zero : new Vector3(_position.x, _position.y, _position.z); 
        GameObject _newObject = UnityEngine.Object.Instantiate(_prefab, _savedPosition, Quaternion.identity, parent);

        return _newObject;
    }
}
