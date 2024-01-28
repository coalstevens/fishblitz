using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    private static CameraManager _instance;
    public static CameraManager Instance
    {
        get
        {
            // If the instance doesn't exist, find it in the scene
            if (_instance == null)
            {
                _instance = FindObjectOfType<CameraManager>();

                if (_instance == null)
                {
                    Debug.LogError("Cameras object does not exist");
                }
            }

            return _instance;
        }
    }
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject); // Optional: Keeps the GameObject with the singleton alive between scenes
        }
        else
        {
            Destroy(gameObject); // Ensures that only one instance of the singleton exists
        }
    }
}
