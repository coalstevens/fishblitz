using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;

public class Fish : MonoBehaviour
{
    public string validSceneName;
    // Start is called before the first frame update
    [Header("Fish Catching Behavior")]
    public float PlayDuration = 5f;
    public int NumTriggers; // always a trigger press at the end 
    public float MinimumTriggerGap;
    public float SpecialGap;
    public enum modifier { normal, doubles, triples, mega };
    public modifier gameModifier;
}
