using UnityEngine;

public class UIScalerSync : MonoBehaviour
{
    [SerializeField] private DynamicPixelPerfectCamera targetCamera;
    [SerializeField] private Canvas canvas;

    private void Awake()
    {
        if (targetCamera == null)
        {
            GameObject cameraObj = GameObject.FindGameObjectWithTag("MainCamera");
            if (cameraObj != null)
            {
                targetCamera = cameraObj.GetComponent<DynamicPixelPerfectCamera>();
            }
        }

        if (canvas == null)
        {
            canvas = GetComponent<Canvas>();
        }

        if (canvas != null && targetCamera != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = targetCamera.GetComponent<Camera>();
        }
    }

    private void Update()
    {
        if (targetCamera == null || canvas == null) return;

        Camera cam = targetCamera.GetComponent<Camera>();
        if (cam == null) return;

        float orthoSize = cam.orthographicSize;
        float unitsToPixels = Screen.height / (2f * orthoSize);
        canvas.scaleFactor = unitsToPixels / targetCamera.PixelsPerUnit;
    }
}
