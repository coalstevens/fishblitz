using UnityEngine;
using UnityEngine.UI;

public class AdditiveUIScaler : MonoBehaviour
{
    public float pixelsPerUnit = 32;
    private Canvas canvas;
    private CanvasScaler canvasScaler;
    private DynamicPixelPerfectCamera _pixelCamera;

    void Awake()
    {
        canvas = GetComponent<Canvas>();
        canvasScaler = GetComponent<CanvasScaler>();

        Camera targetCamera = Camera.main;
        if (targetCamera == null)
        {
            Debug.LogError("No main camera found for UI scaling!");
            return;
        }

        _pixelCamera = targetCamera.GetComponent<DynamicPixelPerfectCamera>();

        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = targetCamera;

        UpdateScale();
    }
    void Update()
    {
        UpdateScale();
    }

    void UpdateScale()
    {
        if (_pixelCamera != null && canvasScaler != null)
        {
            canvasScaler.scaleFactor = _pixelCamera.ScaleFactor;
        }
    }
}
