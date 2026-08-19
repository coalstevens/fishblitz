using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class PlayerCaptureLogs : MonoBehaviour, ISaveable
{
    public string SaveableId => "PlayerLogs";

    [SerializeField] private CaptureLog _birdingLog = new();
    [SerializeField] private CaptureLog _fishingLog = new();

    public CaptureLog BirdingLog => _birdingLog;
    public CaptureLog FishingLog => _fishingLog;

    [System.Serializable]
    private class State
    {
        public List<CaptureLog.CaptureEntry> BirdingLogEntries = new();
        public List<CaptureLog.CaptureEntry> FishingLogEntries = new();
    }

    public string CaptureState()
    {
        var _state = new State();
        if (_birdingLog != null)
            _state.BirdingLogEntries = new List<CaptureLog.CaptureEntry>(_birdingLog.CaptureTable);
        if (_fishingLog != null)
            _state.FishingLogEntries = new List<CaptureLog.CaptureEntry>(_fishingLog.CaptureTable);
        return JsonConvert.SerializeObject(_state);
    }

    public void RestoreState(string json)
    {
        var _state = JsonConvert.DeserializeObject<State>(json);
        if (_birdingLog != null && _state.BirdingLogEntries != null)
            _birdingLog.CaptureTable = _state.BirdingLogEntries;
        if (_fishingLog != null && _state.FishingLogEntries != null)
            _fishingLog.CaptureTable = _state.FishingLogEntries;
    }

    public void ResetState() { }
}
