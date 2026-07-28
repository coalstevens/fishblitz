using System;
using Newtonsoft.Json;
using UnityEngine;

public class PlayerSceneData : MonoBehaviour, ISaveableComponent
{
    public string ComponentId => "PlayerScene";

    public static Vector3 PendingSpawnPosition;
    public static bool HasPendingSpawn;

    public Vector3 SceneSpawnPosition = new Vector3(0, 0);
    public string SceneOnAwake;

    [Serializable]
    private class State
    {
        public float PositionX;
        public float PositionY;
        public float PositionZ;
        public string SceneOnAwake;
    }

    public string CaptureStateAsJson()
    {
        var _state = new State
        {
            PositionX = transform.position.x,
            PositionY = transform.position.y,
            PositionZ = transform.position.z,
            SceneOnAwake = SceneOnAwake
        };
        return JsonConvert.SerializeObject(_state);
    }

    public void RestoreStateFromJson(string json)
    {
        var _state = JsonConvert.DeserializeObject<State>(json);
        Vector3 pos = new Vector3(_state.PositionX, _state.PositionY, _state.PositionZ);
        transform.position = pos;
        SceneSpawnPosition = pos;
        SceneOnAwake = _state.SceneOnAwake;
    }
}
