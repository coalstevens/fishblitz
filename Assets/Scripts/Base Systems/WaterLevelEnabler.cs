using System.Collections.Generic;
using UnityEngine;

public class WaterLevelEnabler : MonoBehaviour
{
    [SerializeField] private List<GameObject> _flood = new();
    [SerializeField] private List<GameObject> _full = new();
    [SerializeField] private List<GameObject> _shallow = new();
    [SerializeField] private List<GameObject> _puddles = new();
    [SerializeField] private List<GameObject> _postFlood = new();

    void Start()
    {
        DisableAllObjects();
        switch (WorldState.WaterState.Value)
        {
            case WorldState.WaterStates.Flood:
                EnableObjects(_flood);
                break;
            case WorldState.WaterStates.Full:
                EnableObjects(_full);
                break;
            case WorldState.WaterStates.Shallow:
                EnableObjects(_shallow);
                break;
            case WorldState.WaterStates.Puddles:
                EnableObjects(_puddles);
                break;
            case WorldState.WaterStates.PostFlood:
                EnableObjects(_postFlood);
                break;
        }
    }

    private void EnableObjects(List<GameObject> objects)
    {
        foreach (var obj in objects)
            obj.SetActive(true);
    }

    private void DisableAllObjects()
    {
        foreach (var obj in _flood)
            obj.SetActive(false);
        foreach (var obj in _full)
            obj.SetActive(false);
        foreach (var obj in _shallow)
            obj.SetActive(false);
        foreach (var obj in _puddles)
            obj.SetActive(false);
        foreach (var obj in _postFlood)
            obj.SetActive(false);
    }
}
