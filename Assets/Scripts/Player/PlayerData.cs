using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public Vector3 SceneSpawnPosition = new Vector3(0,0);
    private static PlayerData _instance;

    public static PlayerData Instance {
        get {
            if (_instance == null) {
                _instance = FindObjectOfType<PlayerData>();
                if (_instance == null) {
                    GameObject singleton = new GameObject(typeof(PlayerData).Name);
                    _instance = singleton.AddComponent<PlayerData>();
                }
                DontDestroyOnLoad(_instance.gameObject);
            }
            return _instance;
        }
    }
    private void Awake() {
        if (_instance != null && _instance!= this) {
            Destroy(this.gameObject);
        }
    }
}
