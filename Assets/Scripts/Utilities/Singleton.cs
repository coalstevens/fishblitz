using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    public static T Instance {
        get {
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
