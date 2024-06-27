using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
 
[Serializable]
public class Logger
{
    [SerializeField]
    private bool _info = false;
 
    [SerializeField]
    private bool _verbose = false;
 
    [SerializeField]
    private string _prefix = "";
 
    // see https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/StyledText.html for valid colors.
    [SerializeField]
    private string _color = "green";
 
    private void LogRichText(object message)
    {
        Debug.Log($"<color={_color}><b>{_prefix}</b></color>: {message}");
    }
 
    public void Warning(object message)
    {
        Debug.LogWarning($"<b>{_prefix}</b>: {message}");
    }
 
    public void Info(object message)
    {
        if (!_info)
        {
            return;
        }
 
        LogRichText(message);
    }
 
    public void Verbose(object message)
    {
        if (!_info)
        {
            return;
        }
 
        if (!_verbose)
        {
            return;
        }
 
        LogRichText(message);
    }

    public static string FloatArrayToString(float[] array) {
        return string.Join(", ", array.Select(x => x.ToString()));
    }

    public static string GetGameObjectNames<T>(T[] values) where T : MonoBehaviour
    {
        return string.Join(", ", values.Select(x => x.gameObject.name));
    }
}