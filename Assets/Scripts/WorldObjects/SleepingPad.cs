using System.Collections.Generic;
using UnityEngine;

public class SleepingPad : MonoBehaviour, InteractInput.IInteractable, ISceneSaveable
{
    PlayerEnergyManager _playerEnergyManager;
    private const string IDENTIFIER = "Sleeping Pad";

    private void Awake()
    {
        _playerEnergyManager = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerEnergyManager>();
    }

    public bool CursorInteract(Vector3 cursorLocation)
    {
        _playerEnergyManager.Sleep();
        return true;
    }

    private string _persistentID;
    public string PrefabId => IDENTIFIER;
    public string PersistentID { get => _persistentID; set => _persistentID = value; }

    public string CaptureState() {
        return null;
    }

    public void RestoreState(string json)
    {
        // no extended data to load
    }

    public void ResetState() { }
}
