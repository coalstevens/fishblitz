using UnityEngine;

[DefaultExecutionOrder(-100)]
[RequireComponent(typeof(Canvas))]
public class AssignUICameraAtRuntime : MonoBehaviour
{
    private Canvas canvas;

    void Start()
    {
        canvas = GetComponent<Canvas>();
        AssignCamera();
    }

    void AssignCamera()
    {
        // Find UI camera by tag
        GameObject camObj = GameObject.FindWithTag("UICamera");
        if (camObj == null)
        {
            Debug.LogWarning("AssignUICameraAtRuntime: No camera found with tag 'UICamera'.");
            return;
        }

        Camera uiCam = camObj.GetComponent<Camera>();
        if (uiCam == null)
        {
            Debug.LogWarning("AssignUICameraAtRuntime: Tagged object has no Camera component.");
            return;
        }

        // Set the camera
        canvas.worldCamera = uiCam;
    }
}