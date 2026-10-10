using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public class SceneReferenceBuildGuard : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        var issues = SceneReferenceEditorUtility.SyncAll();
        AssetDatabase.SaveAssets();

        if (issues.Count == 0) return;

        var sb = new StringBuilder();
        sb.AppendLine("Build blocked: stale or invalid SceneReference(s) detected.");
        foreach (var issue in issues)
            sb.AppendLine($"  {issue.AssetPath} ({issue.FieldPath}): name='{issue.SceneName}' guid='{issue.SceneGuid}' — {issue.Reason}");

        throw new BuildFailedException(sb.ToString());
    }
}
