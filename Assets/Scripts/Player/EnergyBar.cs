using System;
using UnityEngine;
using UnityEngine.UI;

public class EnergyBar : MonoBehaviour
{
    [SerializeField] PlayerData _playerData;
    private Image _energyBar;
    private Action _unsubscribe;
    private float _maxWidth; // max width is currently the starting width of the sprite 

    private void OnEnable()
    {
        _energyBar = GetComponent<Image>();
        _maxWidth = _energyBar.rectTransform.rect.width;
        _unsubscribe = _playerData.CurrentEnergy.OnChange(curr => UpdateEnergyBar(curr));
        UpdateEnergyBar(_playerData.CurrentEnergy.Value);
    }

    private void OnDisable()
    {
        _unsubscribe?.Invoke();
    }

    private void UpdateEnergyBar(int energy)
    {
        float newWidth = Mathf.Lerp(0, _maxWidth, (float)energy / _playerData.MaxEnergy);
        var sizeDelta = _energyBar.rectTransform.sizeDelta;
        sizeDelta.x = newWidth;
        _energyBar.rectTransform.sizeDelta = sizeDelta;
    }
}
