using UnityEngine;

[System.Serializable]
public class WeightedScene
{
    public SceneReference Scene;
    [Range(0f, 1f)] public float Weight;
}
