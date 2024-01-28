using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventSystemManager : MonoBehaviour
{
    private static EventSystemManager _instance;
    public static EventSystemManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<EventSystemManager>();

                if (_instance == null)
                {
                    Debug.LogError("EventSystemManager object does not exist");
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
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject); 
        }
    }
}
