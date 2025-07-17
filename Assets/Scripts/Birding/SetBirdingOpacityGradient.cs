using NUnit.Framework;
using UnityEngine;

public class SetBirdingOpacityGradient : MonoBehaviour
{
    [SerializeField] private Transform _gradientOriginPosition;
    [SerializeField] private Material _affectedMaterial;

    private void Start()
    {
        Assert.IsNotNull(_gradientOriginPosition, "Transform is not set.");
        Assert.IsNotNull(_affectedMaterial, "Material is not set.");
        Assert.IsTrue(_affectedMaterial.HasProperty("_GradientOriginPosition"), "Material does not have _GradientOriginPosition property.");
    }

    private void Update()
    {
        _affectedMaterial.SetVector("_GradientOriginPosition", _gradientOriginPosition.position);
    }

    private void OnDestroy()
    {
        _affectedMaterial.SetVector("_GradientOriginPosition", Vector3.zero);
    }
}
