using UnityEngine;

[System.Serializable]
public class WeightedScene
{
    public SceneNames Scene;
    [Range(0f, 1f)] public float Weight;
}
