using Newtonsoft.Json;
using UnityEngine;

public class Spruce : TreePlant, ISceneSaveable
{

    private const string IDENTIFIER = "Spruce";
    private class SpruceSaveData
    {
        public TreeStates TreeState;
    }

    private string _persistentID;
    public string PrefabId => IDENTIFIER;
    public string PersistentID { get => _persistentID; set => _persistentID = value; }

    public string CaptureState()
    {
        var _extendedData = new SpruceSaveData
        {
            TreeState = _treeState.Value
        };
        return JsonConvert.SerializeObject(_extendedData);
    }

    public void RestoreState(string json)
    {
        var _extendedData = JsonConvert.DeserializeObject<SpruceSaveData>(json);
        _treeState.Value = _extendedData.TreeState;
    }

    public void ResetState() { }
}
