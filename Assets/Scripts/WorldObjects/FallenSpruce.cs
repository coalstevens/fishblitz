using UnityEditor.SearchService;
using UnityEngine;

public class FallenSpruce : FallenTree, SceneSaveLoadManager.ISaveable
{
    [SerializeField] private string _identifier;
    private class FallenSpruceSaveData
    {
        public FallenTreeStates State;
    }

    public SaveData Save()
    {
        var _extendedData = new FallenSpruceSaveData()
        {
            State = _state.Value,
        };

        var _saveData = new SaveData();
        _saveData.AddIdentifier(_identifier);
        _saveData.AddTransformPosition(transform.position);
        _saveData.AddExtendedSaveData<FallenSpruceSaveData>(_extendedData);
        return _saveData;
    }

    public void Load(SaveData saveData)
    {
        var _extendedData = saveData.GetExtendedSaveData<FallenSpruceSaveData>();
        _identifier = saveData.Identifier;
        _state.Value = _extendedData.State;
        if (_state.Value == FallenTreeStates.Idle)
            StopAnimation(); 
    }
}

