using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class EnergyBar : MonoBehaviour
{
    private PlayerEnergyManager _energyManager;
    private Image _energyBar;
    private Action _unsubscribe;
    private float _maxWidth;

    private void OnEnable()
    {
        _energyManager = FindFirstObjectByType<PlayerEnergyManager>();
        _energyBar = GetComponent<Image>();
        _maxWidth = _energyBar.rectTransform.rect.width;
        _unsubscribe = _energyManager.CurrentEnergy.OnChange(curr => UpdateEnergyBar(curr));
        UpdateEnergyBar(_energyManager.CurrentEnergy.Value);
    }

    private void OnDisable()
    {
        _unsubscribe?.Invoke();
    }

    private void UpdateEnergyBar(int energy)
    {
        float newWidth = Mathf.Lerp(0, _maxWidth, (float)energy / _energyManager.MaxEnergy);
        var sizeDelta = _energyBar.rectTransform.sizeDelta;
        sizeDelta.x = newWidth;
        _energyBar.rectTransform.sizeDelta = sizeDelta;
    }
}
