using UnityEngine;

[CreateAssetMenu(fileName = "PlayerMovementData", menuName = "Player/MovementData")]
public class PlayerMovementData : ScriptableObject
{
    [Header("Move Speeds")]
    public float DefaultMoveSpeed = 3.5f;
    public float WheelbarrowMoveSpeed = 3.5f;

    [Header("Base Accelerations")]
    public float BaseAcceleration = 3f;
    public float BaseDeceleration = 20f;
    public float BaseWheelbarrowAcceleration = 10f;
    public float BaseWheelbarrowDeceleration = 20f;

    [Header("Carrying")]
    public float CarryingPerObjectAccelReduction = 1f;
    public float CarryingPerObjectMoveSpeedReduction = 0.25f;

    [Header("Wheelbarrow")]
    public float WheelbarrowPerObjectAccelReduction = 1f;
    public float WheelbarrowPerObjectMoveSpeedReduction = 0.25f;
}
