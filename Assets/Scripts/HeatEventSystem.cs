using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum Temperature {Freezing, Cold, Neutral, Warm, Hot};

public class HeatEventSystem : MonoBehaviour {
    public delegate void TemperatureChangeEventHandler(IHeatSource heatSource, Temperature newTemp);
    public event TemperatureChangeEventHandler TemperatureChange;
    public void TriggerTemperatureChange(IHeatSource heatSource, Temperature newTemp) {
        // Check if there are any subscribers (listeners) to the event.
        if (TemperatureChange != null)
        {
            // Invoke the event, passing 'this' as the sender and EventArgs.Empty as the event arguments.
            TemperatureChange(heatSource, newTemp);
        }
    }
    /*
    public ObjectState sharedObjectState;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        LoadObjectState();
    }

    private void OnSceneUnloaded(Scene scene)
    {
        SaveObjectState();
    }

    private void SaveObjectState()
    {
        Saveable[] saveables = FindObjectsOfType<Saveable>();

        foreach (Saveable saveable in saveables)
        {
            saveable.SaveState();
        }
    }

    private void LoadObjectState()
    {
        Saveable[] saveables = FindObjectsOfType<Saveable>();

        foreach (Saveable saveable in saveables)
        {
            saveable.LoadState();
        }
    }*/
}
/*
public class LoadRoom : MonoBehaviour {
    void ProcessHeatChangesSinceRoomExit() {
        IHeatSource[] _heatSources = FindObjectsOfType<MonoBehaviour>().OfType<IHeatSource>().ToArray();
        foreach (IHeatSource _heatSource in _heatSources) {
            // _heatSource.state = scriptableobject.previous state
            // _heatSource.timers = load all timers;
        }
        for (int i = 0; i < elapsedGameMinutes; i++) {
            foreach(IHeatSource _heatSource in _heatSources) {
                _heatSource.OnGameMinuteChange();
            }
        }
    }

}
*/