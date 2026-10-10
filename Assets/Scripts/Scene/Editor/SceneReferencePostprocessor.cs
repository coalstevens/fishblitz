using System.Collections.Generic;
using UnityEditor;

public class SceneReferencePostprocessor : AssetPostprocessor
{
    private static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        if (movedAssets == null || movedAssets.Length == 0) return;

        var renamedSceneGuids = new HashSet<string>();
        foreach (string path in movedAssets)
        {
            if (!path.EndsWith(".unity")) continue;

            string guid = AssetDatabase.AssetPathToGUID(path);
            if (!string.IsNullOrEmpty(guid))
                renamedSceneGuids.Add(guid);
        }

        if (renamedSceneGuids.Count > 0)
            SceneReferenceEditorUtility.SyncGuids(renamedSceneGuids);
    }
}
