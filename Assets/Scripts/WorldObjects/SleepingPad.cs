using System.Collections.Generic;
using UnityEngine;

public class SleepingPad : MonoBehaviour, InteractInput.IInteractable, SaveData.ISaveable
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

    public SaveData Save() {
        var _saveData = new SaveData();
        _saveData.AddIdentifier(IDENTIFIER);
        _saveData.AddTransformPosition(transform.position);
        return _saveData;
    }

    public void Load(SaveData saveData)
    {
        // no extended data to load
    }
}
