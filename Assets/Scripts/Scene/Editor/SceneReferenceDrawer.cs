using System.IO;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SceneReference))]
public class SceneReferenceDrawer : PropertyDrawer
{
    private const float WarningHeight = 38f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty nameProp = property.FindPropertyRelative("_sceneName");
        SerializedProperty guidProp = property.FindPropertyRelative("_sceneGuid");

        SceneAsset sceneAsset = ResolveSceneAsset(nameProp.stringValue, guidProp.stringValue);

        Rect fieldRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

        EditorGUI.BeginProperty(position, label, property);
        EditorGUI.BeginChangeCheck();

        SceneAsset newAsset = (SceneAsset)EditorGUI.ObjectField(fieldRect, label, sceneAsset, typeof(SceneAsset), false);

        if (EditorGUI.EndChangeCheck())
        {
            if (newAsset == null)
            {
                nameProp.stringValue = string.Empty;
                guidProp.stringValue = string.Empty;
            }
            else
            {
                nameProp.stringValue = newAsset.name;
                guidProp.stringValue = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(newAsset));
            }
        }
        EditorGUI.EndProperty();

        if (!string.IsNullOrEmpty(nameProp.stringValue) && !IsInBuildSettings(nameProp.stringValue))
        {
            Rect warningRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2f, position.width, WarningHeight);
            EditorGUI.HelpBox(warningRect, $"'{nameProp.stringValue}' is not in Build Settings and will fail to load at runtime.", MessageType.Warning);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty nameProp = property.FindPropertyRelative("_sceneName");
        if (!string.IsNullOrEmpty(nameProp.stringValue) && !IsInBuildSettings(nameProp.stringValue))
            return EditorGUIUtility.singleLineHeight + WarningHeight + 2f;
        return EditorGUIUtility.singleLineHeight;
    }

    private static SceneAsset ResolveSceneAsset(string sceneName, string sceneGuid)
    {
        if (!string.IsNullOrEmpty(sceneGuid))
        {
            string path = AssetDatabase.GUIDToAssetPath(sceneGuid);
            SceneAsset asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            if (asset != null) return asset;
        }

        if (!string.IsNullOrEmpty(sceneName))
        {
            foreach (string guid in AssetDatabase.FindAssets($"t:Scene {sceneName}"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(path) == sceneName)
                    return AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            }
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
