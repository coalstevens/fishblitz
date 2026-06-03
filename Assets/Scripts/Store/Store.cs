using System.Collections.Generic;
using UnityEngine;

public abstract class Store : ScriptableObject
{
    private static Dictionary<System.Type, Store> _cache = new();

    public static T Get<T>() where T : Store
    {
        System.Type key = typeof(T);
        if (_cache.TryGetValue(key, out Store cached))
            return (T)cached;

        T[] found = Resources.LoadAll<T>("Stores");

        if (found.Length == 0)
        {
            Debug.LogError($"Store.Get<{key.Name}>(): No instance in Resources/Stores/");
            _cache[key] = null;
            return null;
        }
        if (found.Length > 1)
        {
            Debug.LogError($"Store.Get<{key.Name}>(): {found.Length} instances in Resources/Stores/ — only one allowed");
            _cache[key] = null;
            return null;
        }

        _cache[key] = found[0];
        return found[0];
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset() => _cache.Clear();
}
