using System;
using System.Collections.Generic;
using UnityEngine;

public class ActiveWhenRaining : MonoBehaviour
{
    [SerializeField] private Rain _rainManager;
    [SerializeField] private List<Component> _components = new();
    private Action _unsubscribe;

    private void OnEnable()
    {
        _unsubscribe = _rainManager.State.OnChange((_, curr) => OnRainStateChange(curr));
        OnRainStateChange(_rainManager.State.Value);
    }

    private void OnDisable() 
    {
        _unsubscribe();
    }

    private void OnRainStateChange(Rain.States newState)
    {
        bool _isNotRaining = newState == Rain.States.NoRain;

        foreach (var _component in _components) {
            if (_isNotRaining)
                DisableComponent(_component);
            else
                EnableComponent(_component);
        }
    }

    public void EnableComponent(Component component)
    {
        if (component is Behaviour behaviour)
            behaviour.enabled = true; 
        else if (component is Collider collider)
            collider.enabled = true; 
        else if (component is Renderer renderer)
            renderer.enabled = true; 
        else if (component is ParticleSystem particleSystem)
            particleSystem.Play();
        else
            Debug.LogError("This component type is not handled yet.");
    }

    public void DisableComponent(Component component)
    {
        if (component is Behaviour behaviour)
            behaviour.enabled = false; 
        else if (component is Collider collider)
            collider.enabled = false; 
        else if (component is Renderer renderer)
            renderer.enabled = false; 
        else if (component is ParticleSystem particleSystem)
            particleSystem.Stop();
        else
            Debug.LogError("This component type is not handled yet.");
    }
}
