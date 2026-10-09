using System.IO;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SceneNameAttribute))]
public class SceneNameDrawer : PropertyDrawer
{
    private const float WarningHeight = 38f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.String)
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        Rect fieldRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

        EditorGUI.BeginProperty(position, label, property);
        EditorGUI.BeginChangeCheck();

        SceneAsset sceneAsset = ResolveSceneAsset(property.stringValue);
        SceneAsset newAsset = (SceneAsset)EditorGUI.ObjectField(fieldRect, label, sceneAsset, typeof(SceneAsset), false);

        if (EditorGUI.EndChangeCheck())
            property.stringValue = newAsset != null ? newAsset.name : string.Empty;

        EditorGUI.EndProperty();

        if (!string.IsNullOrEmpty(property.stringValue) && !IsInBuildSettings(property.stringValue))
        {
            Rect warningRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2f, position.width, WarningHeight);
            EditorGUI.HelpBox(warningRect, $"'{property.stringValue}' is not in Build Settings and will fail to load at runtime.", MessageType.Warning);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (property.propertyType == SerializedPropertyType.String &&
            !string.IsNullOrEmpty(property.stringValue) &&
            !IsInBuildSettings(property.stringValue))
            return EditorGUIUtility.singleLineHeight + WarningHeight + 2f;
        return EditorGUIUtility.singleLineHeight;
    }

    private static SceneAsset ResolveSceneAsset(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return null;

        foreach (string guid in AssetDatabase.FindAssets($"t:Scene {sceneName}"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetFileNameWithoutExtension(path) == sceneName)
                return AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
        }

        return null;
    }

    private static bool IsInBuildSettings(string sceneName)
    {
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (Path.GetFileNameWithoutExtension(scene.path) == sceneName)
                return true;
        }
        return false;
    }
}
