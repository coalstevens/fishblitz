using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;

public class Fish : MonoBehaviour
{
    public string validSceneName;
    // Start is called before the first frame update
    [Header("Fish Catching Behavior")]
    public float playDuration = 5f;
    public int numTriggers; // always a trigger press at the end 
    public float minimumTriggerGap;
    public float specialGap;
    public enum modifier { normal, doubles, triples, mega };
    public modifier gameModifier;
}
