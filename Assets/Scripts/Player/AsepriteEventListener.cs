using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class AsepriteEventListener : MonoBehaviour
{
    [Serializable]
    private class TransformLocalData
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;

        public TransformLocalData(Transform transform)
        {
            Position = transform.localPosition;
            Rotation = transform.localRotation;
            Scale = transform.localScale;
        }
    }

    [Serializable]
    private class Effect
    {
        public GameObject Affected;
        public PlayerAnimationEvents EventName;
        public TransformLocalData Offset;
    }

    [SerializeField] private List<Effect> _effects = new();
    private List<TransformLocalData> _originals = new();
    private Animator _animator;
    private enum PlayerAnimationEvents { AIRBORNE_1PX, AIRBORNE_2PX, GROUNDED, HALF_SQUAT, SQUAT, PICKUP_1, PICKUP_2, PICKUP_3, PICKUP_4}
    private PlayerAnimationEvents _lastEvent = PlayerAnimationEvents.GROUNDED;

    private void OnEnable()
    {
        _animator = GetComponent<Animator>();
        Assert.IsNotNull(_animator);
        foreach (var _effect in _effects)
            _originals.Add(new TransformLocalData(_effect.Affected.transform));
    }

    private void OnGrounded()
    {
        ApplyEventEffects(PlayerAnimationEvents.GROUNDED);
    }

    private void OnAirborne1px()
    {
        ApplyEventEffects(PlayerAnimationEvents.AIRBORNE_1PX);
    }

    private void OnAirborne2px()
    {
        ApplyEventEffects(PlayerAnimationEvents.AIRBORNE_2PX);
    }

    private void OnHalfSquat()
    {
        ApplyEventEffects(PlayerAnimationEvents.HALF_SQUAT);
    }

    private void OnSquat()
    {
        ApplyEventEffects(PlayerAnimationEvents.SQUAT);
    }
    private void OnPickup1()
    {
        ApplyEventEffects(PlayerAnimationEvents.PICKUP_1);
    }
    private void OnPickup2()
    {
        ApplyEventEffects(PlayerAnimationEvents.PICKUP_2);
    }
    private void OnPickup3()
    {
        ApplyEventEffects(PlayerAnimationEvents.PICKUP_3);
    }
    private void OnPickup4()
    {
        ApplyEventEffects(PlayerAnimationEvents.PICKUP_4);
    }

    private void ApplyEventEffects(PlayerAnimationEvents animationEvent)
    {
        if (_lastEvent == animationEvent) return;

        _lastEvent = animationEvent;
        for (int i = 0; i < _effects.Count; i++)
        {
            if (_effects[i].EventName == animationEvent)
            {
                UpdateTransform(_effects[i], _originals[i]);
            }
        }
    }

    private void UpdateTransform(Effect effect, TransformLocalData original)
    {
        effect.Affected.transform.localPosition = original.Position + effect.Offset.Position;
        effect.Affected.transform.localRotation = Quaternion.Euler(original.Rotation.eulerAngles + effect.Offset.Rotation.eulerAngles);
        effect.Affected.transform.localScale = new Vector3(
            original.Scale.x + effect.Offset.Scale.x,
            original.Scale.y + effect.Offset.Scale.y,
            original.Scale.z + effect.Offset.Scale.z);
    }
}
