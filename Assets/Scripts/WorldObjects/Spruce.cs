public class Spruce : TreePlant, ISaveable
{
    private const string IDENTIFIER = "Spruce";
    private class SpruceSaveData
    {
        public TreeStates TreeState;
    }

    public SaveData Save()
    {
        var _extendedData = new SpruceSaveData
        {
            TreeState = _treeState.Value
        };

        var _saveData = new SaveData();
        _saveData.AddIdentifier(IDENTIFIER);
        _saveData.AddTransformPosition(transform.position);
        _saveData.AddExtendedSaveData<SpruceSaveData>(_extendedData);
        return _saveData;
    }

    public void Load(SaveData saveData)
    {
        var _extendedData = saveData.GetExtendedSaveData<SpruceSaveData>();
        _treeState.Value = _extendedData.TreeState;
    }
}
