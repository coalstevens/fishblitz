using System.Collections.Generic;
using UnityEngine;

public class FishType : MonoBehaviour
{
    public List<string> CatchableSceneNames;
    // Start is called before the first frame update
    [Header("Fish Catching Behavior")]
    public float GameSpeed = 5f;
    public int NumberOfTriggers; // including final trigger at the very top
    public float MinimumTriggerSpacing; // triggers can't be closer together than this
    public float StackedTriggerSpacing; // custom gap for stacked trigger types
    public enum StackedTriggerType { none, doubles, mega };
    public StackedTriggerType GameModifier;
    public bool HasOscillatingTriggers;
    public float OscillatingSpeed;
    [Range(0.0f, 1.0f)] public float OscillationLengthNormalized;
}
