using System;
using UnityEngine;

public enum RainStates { Raining, NotRaining }; 

public class RainManager : Singleton<RainManager>
{
    public RainStates RainState = RainStates.Raining; // SUBSCRIBE WITH EVENTS
    public event Action<RainStates> RainStateChange;
    private GameObject _rainParticleSystem;

    public void Enable()
    {
        _rainParticleSystem.gameObject.SetActive(true);
        RainState = RainStates.Raining;
        RainStateChange.Invoke(RainState);
    }

    public void Disable()
    {
        _rainParticleSystem.gameObject.SetActive(false);
        RainState = RainStates.NotRaining;
        RainStateChange.Invoke(RainState);
    }
}
