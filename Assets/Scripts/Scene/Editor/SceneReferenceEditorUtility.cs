using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class SceneReferenceEditorUtility
{
    private const string SceneReferenceTypeName = nameof(SceneReference);
    private const string NameProperty = "_sceneName";
    private const string GuidProperty = "_sceneGuid";

    public struct ValidationIssue
    {
        public string AssetPath;
        public string FieldPath;
        public string SceneName;
        public string SceneGuid;
        public string Reason;
    }

    public static List<ValidationIssue> SyncAll()
    {
        return Sync(null);
    }

    public static void SyncGuids(HashSet<string> renamedSceneGuids)
    {
        Sync(renamedSceneGuids);
    }

    [MenuItem("Tools/Scene References/Validate")]
    private static void Validate()
    {
        var issues = SyncAll();
        if (issues.Count == 0)
        {
            Debug.Log("Scene References: all valid.");
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine($"Scene References: {issues.Count} issue(s) found:");
        foreach (var issue in issues)
            sb.AppendLine($"  {issue.AssetPath} ({issue.FieldPath}): name='{issue.SceneName}' guid='{issue.SceneGuid}' — {issue.Reason}");
        Debug.LogError(sb.ToString());
    }

    private static List<ValidationIssue> Sync(HashSet<string> guidFilter)
    {
        var issues = new List<ValidationIssue>();
        bool anyChanged = false;

        foreach (string path in GetCandidateAssetPaths())
        {
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);
            if (asset == null) continue;

            var serializedObject = new SerializedObject(asset);
            SerializedProperty iterator = serializedObject.GetIterator();
            bool changed = false;
            bool enterChildren = true;

            while (iterator.Next(enterChildren))
            {
                enterChildren = true;

                if (iterator.propertyType != SerializedPropertyType.Generic || iterator.type != SceneReferenceTypeName)
                    continue;

                SerializedProperty nameProp = iterator.FindPropertyRelative(NameProperty);
                SerializedProperty guidProp = iterator.FindPropertyRelative(GuidProperty);
                if (nameProp == null || guidProp == null) continue;

                string storedGuid = guidProp.stringValue;
                if (guidFilter != null && !guidFilter.Contains(storedGuid)) continue;

                string resolvedGuid = ResolveGuid(storedGuid, nameProp.stringValue, out string resolvedName);

                if (resolvedGuid != null)
                {
                    if (guidProp.stringValue != resolvedGuid)
                    {
                        guidProp.stringValue = resolvedGuid;
                        changed = true;
                    }
                    if (nameProp.stringValue != resolvedName)
                    {
                        nameProp.stringValue = resolvedName;
                        changed = true;
                    }

                    if (guidFilter == null && !IsInBuildSettings(resolvedName))
                    {
                        issues.Add(new ValidationIssue
                        {
                            AssetPath = path,
                            FieldPath = iterator.propertyPath,
                            SceneName = resolvedName,
                            SceneGuid = resolvedGuid,
                            Reason = "scene is not in Build Settings and will fail to load at runtime"
                        });
                    }
                }
                else if (!string.IsNullOrEmpty(nameProp.stringValue) || !string.IsNullOrEmpty(storedGuid))
                {
                    issues.Add(new ValidationIssue
                    {
                        AssetPath = path,
                        FieldPath = iterator.propertyPath,
                        SceneName = nameProp.stringValue,
                        SceneGuid = storedGuid,
                        Reason = "could not resolve scene from name or GUID"
                    });
                }

                enterChildren = false;
            }

            if (changed)
            {
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                PersistAsset(asset);
                anyChanged = true;
            }
        }

        if (anyChanged)
            AssetDatabase.SaveAssets();

        return issues;
    }

    private static string ResolveGuid(string guid, string sceneName, out string resolvedName)
    {
        resolvedName = null;

        if (!string.IsNullOrEmpty(guid))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!string.IsNullOrEmpty(path) && path.EndsWith(".unity"))
            {
                resolvedName = Path.GetFileNameWithoutExtension(path);
                return guid;
            }
        }

        if (!string.IsNullOrEmpty(sceneName))
        {
            foreach (string candidate in AssetDatabase.FindAssets($"t:Scene {sceneName}"))
            {
                string path = AssetDatabase.GUIDToAssetPath(candidate);
                if (!path.EndsWith(".unity")) continue;
                if (Path.GetFileNameWithoutExtension(path) != sceneName) continue;

                resolvedName = sceneName;
                return candidate;
            }
        }

        return null;
    }

    private static void PersistAsset(Object asset)
    {
        if (asset is GameObject prefab)
            PrefabUtility.SavePrefabAsset(prefab);
        else
            EditorUtility.SetDirty(asset);
    }

    private static IEnumerable<string> GetCandidateAssetPaths()
    {
        foreach (string guid in AssetDatabase.FindAssets("t:ScriptableObject"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.StartsWith("Assets/")) yield return path;
        }

        foreach (string guid in AssetDatabase.FindAssets("t:Prefab"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.StartsWith("Assets/")) yield return path;
        }
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
