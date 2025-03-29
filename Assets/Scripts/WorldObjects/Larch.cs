using UnityEngine;

public class Larch : TreePlant, SaveData.ISaveable
{
    private const string IDENTIFIER = "Larch";
    private class LarchSaveData
    {
        public TreeStates TreeState;
    }

    public SaveData Save()
    {
        var _extendedData = new LarchSaveData()
        {
            TreeState = _treeState.Value,
        };

        var _saveData = new SaveData();
        _saveData.AddIdentifier(IDENTIFIER);
        _saveData.AddTransformPosition(transform.position);
        _saveData.AddExtendedSaveData<LarchSaveData>(_extendedData);
        return _saveData;
    }

    public void Load(SaveData saveData)
    {
        var _extendedData = saveData.GetExtendedSaveData<LarchSaveData>();
        _treeState.Value = _extendedData.TreeState;
    }
}
