using NUnit.Framework;
using UnityEngine;

public class BirdOutliner : MonoBehaviour
{
    [SerializeField] private Transform _frameMask;

    private void OnEnable()
    {
        Assert.IsNotNull(_frameMask, "FrameMask is not assigned in the Inspector.");
    }

    private BirdMaterialController GetMaterialController(Collider2D other)
    {
        if (other.TryGetComponent<BirdBrain>(out var bird))
        {
            return bird.gameObject.GetComponentInChildren<BirdMaterialController>();
        }
        return null;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        BirdMaterialController _materialController = GetMaterialController(other);
        if (_materialController == null)
            return;

        Vector2 birdLocalPos = _frameMask.InverseTransformPoint(other.transform.position);
        if (birdLocalPos.x > _frameMask.localPosition.x)
        {
            _materialController.TryChangeOutlineColor(BirdMaterialController.OutlineColor.Base, false);
        }
        else
        {
            _materialController.TryChangeOutlineColor(BirdMaterialController.OutlineColor.Mask, false);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        BirdMaterialController _materialController = GetMaterialController(other);
        if (_materialController == null)
            return;

        _materialController.TryChangeOutlineColor(BirdMaterialController.OutlineColor.Mask, false);
    }
}