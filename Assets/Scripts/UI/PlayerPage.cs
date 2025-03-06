using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPage : MonoBehaviour, GameMenuManager.IGameMenuPage
{
    [SerializeField] private Image _proteinBar;
    [SerializeField] private Image _carbsBar;
    [SerializeField] private Image _nutrientsBar;
    [SerializeField] private PlayerData _playerData;
    [SerializeField] private TextMeshProUGUI _healthText;
    [SerializeField] private TextMeshProUGUI _wetnessText;
    [SerializeField] private TextMeshProUGUI _temperatureText;
    [SerializeField] private float _maxBarWidth; // max width is currently the starting width of the sprite 

    public void DisableCursor()
    {
        // do nothing
    }

    public void EnableCursor()
    {
        // do nothing
    }

    public void LoadPage()
    {
        gameObject.SetActive(true);
        UpdateDietBars();
        UpdatePlayerStatus();
    }

    private void UpdatePlayerStatus()
    {
        _healthText.text = "Healthy";
        _wetnessText.text = _playerData.WetnessState.Value switch
        {
            PlayerData.WetnessStates.Wet => "Wet",
            PlayerData.WetnessStates.Dry => "Dry",
            PlayerData.WetnessStates.Drying => "Drying",
            PlayerData.WetnessStates.Wetting => "Wetting",
            _ => "Unknown"
        };
        _temperatureText.text = _playerData.ActualPlayerTemperature.Value.ToString();
    }

    private void UpdateDietBars()
    {
        float proteinWidth = Mathf.Lerp(0, _maxBarWidth, (float)_playerData.TodaysProtein / PlayerData.PROTEIN_REQUIRED_DAILY);
        var proteinSizeDelta = _proteinBar.rectTransform.sizeDelta;
        proteinSizeDelta.x = proteinWidth;
        _proteinBar.rectTransform.sizeDelta = proteinSizeDelta;

        float carbsWidth = Mathf.Lerp(0, _maxBarWidth, (float)_playerData.TodaysCarbs / PlayerData.CARBS_REQUIRED_DAILY);
        var carbsSizeDelta = _carbsBar.rectTransform.sizeDelta;
        carbsSizeDelta.x = carbsWidth;
        _carbsBar.rectTransform.sizeDelta = carbsSizeDelta;

        float nutrientsWidth = Mathf.Lerp(0, _maxBarWidth, (float)_playerData.TodaysNutrients / PlayerData.NUTRIENTS_REQUIRED_DAILY);
        var nutrientsSizeDelta = _nutrientsBar.rectTransform.sizeDelta;
        nutrientsSizeDelta.x = nutrientsWidth;
        _nutrientsBar.rectTransform.sizeDelta = nutrientsSizeDelta;
    }

    public void UnloadPage()
    {
        gameObject.SetActive(false);
    }

    public bool MoveCursor(Vector2 inputDirection)
    {
        return false;
    }

    public void Select()
    {
        // do nothing
    }

}
