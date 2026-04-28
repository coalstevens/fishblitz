using UnityEngine;
using Unity.Cinemachine;

[ExecuteAlways]
[SaveDuringPlay]
public class CinemachinePixelSnap : CinemachineExtension
{
    public int pixelsPerUnit = 32;

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage,
        ref CameraState state,
        float deltaTime)
    {
        // Only snap at the final stage
        if (stage != CinemachineCore.Stage.Finalize)
            return;

        float unitsPerPixel = 1f / pixelsPerUnit;

        // Get the camera position from the CameraState
        Vector3 pos = state.RawPosition;

        // Snap to nearest pixel
        pos.x = Mathf.Round(pos.x / unitsPerPixel) * unitsPerPixel;
        pos.y = Mathf.Round(pos.y / unitsPerPixel) * unitsPerPixel;

        // Update the camera position
        state.RawPosition = pos;
    }
}