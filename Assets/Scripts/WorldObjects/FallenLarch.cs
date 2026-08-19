using Newtonsoft.Json;
using UnityEngine;

public class FallenLarch : FallenTree, ISceneSaveable
{
    [SerializeField] private string _identifier;
    private class FallenLarchSaveData
    {
        public FallenTreeStates State;
    }

    private string _persistentID;
    public string PrefabId => _identifier;
    public string PersistentID { get => _persistentID; set => _persistentID = value; }

    public string CaptureState()
    {
        var _extendedData = new FallenLarchSaveData()
        {
            State = _state.Value,
        };
        return JsonConvert.SerializeObject(_extendedData);
    }

    public void RestoreState(string json)
    {
        var _extendedData = JsonConvert.DeserializeObject<FallenLarchSaveData>(json);
        _state.Value = _extendedData.State;
        if (_state.Value == FallenTreeStates.Idle)
            StopAnimation(); 
    }

    public void ResetState() { }
}
