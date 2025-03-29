using UnityEngine;

public class FallenLarch : FallenTree, SaveData.ISaveable
{
    [SerializeField] private string _identifier;
    private class FallenLarchSaveData
    {
        public FallenTreeStates State;
    }

    public SaveData Save()
    {
        var _extendedData = new FallenLarchSaveData()
        {
            State = _state.Value,
        };

        var _saveData = new SaveData();
        _saveData.AddIdentifier(_identifier);
        _saveData.AddTransformPosition(transform.position);
        _saveData.AddExtendedSaveData<FallenLarchSaveData>(_extendedData);
        return _saveData;
    }

    public void Load(SaveData saveData)
    {
        var _extendedData = saveData.GetExtendedSaveData<FallenLarchSaveData>();
        _identifier = saveData.Identifier;
        _state.Value = _extendedData.State;
        if (_state.Value == FallenTreeStates.Idle)
            StopAnimation(); 
    }
}
