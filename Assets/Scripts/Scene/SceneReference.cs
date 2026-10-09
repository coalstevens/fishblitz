using System;
using UnityEngine;

[Serializable]
public struct SceneReference : IEquatable<SceneReference>
{
    [SerializeField] private string _sceneName;
    [SerializeField] private string _sceneGuid;

    public string SceneName => _sceneName ?? string.Empty;
    public string SceneGuid => _sceneGuid ?? string.Empty;
    public bool IsValid => !string.IsNullOrEmpty(_sceneName);

    public SceneReference(string sceneName)
    {
        _sceneName = sceneName;
        _sceneGuid = null;
    }

    public SceneReference(string sceneName, string sceneGuid)
    {
        _sceneName = sceneName;
        _sceneGuid = sceneGuid;
    }

    public override string ToString() => SceneName;

    public bool Equals(SceneReference other) =>
        string.Equals(_sceneName, other._sceneName, StringComparison.Ordinal);

    public override bool Equals(object obj) => obj is SceneReference other && Equals(other);

    public override int GetHashCode() => _sceneName?.GetHashCode() ?? 0;

    public static bool operator ==(SceneReference a, SceneReference b) => a.Equals(b);

    public static bool operator !=(SceneReference a, SceneReference b) => !a.Equals(b);
}
