using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    public static T Instance {
        get {
            if (_instance == null) {
                Debug.LogError("This singleton property was accessed before awake was called");
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null) {
            _instance = this as T;
            // Debug.Log("Singleton created:" + gameObject.name);
            DontDestroyOnLoad(gameObject);
        }
        else {
            Debug.Log("Singleton class destroyed game object: " + gameObject.name);
            Destroy(gameObject);
        }
    }
}
