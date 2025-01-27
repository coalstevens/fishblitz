using UnityEngine;
using UnityEngine.UI;

public class PlayerPage : MonoBehaviour, GameMenuManager.IGameMenuPage
{
    [SerializeField] private Image _proteinBar;
    [SerializeField] private Image _carbsBar;
    [SerializeField] private Image _nutrientsBar;
    [SerializeField] private PlayerData _playerData;
    private float _maxBarWidth; // max width is currently the starting width of the sprite 

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
        _maxBarWidth = _proteinBar.rectTransform.rect.width;
        UpdateDietBars();
    }

    private void UpdateDietBars()
    {
        float proteinWidth = Mathf.Lerp(0, _maxBarWidth, (float)_playerData.TodaysProtein/ PlayerData.PROTEIN_REQUIRED_DAILY);
        var proteinSizeDelta = _proteinBar.rectTransform.sizeDelta;
        proteinSizeDelta.x = proteinWidth;
        _proteinBar.rectTransform.sizeDelta = proteinSizeDelta;

        float carbsWidth = Mathf.Lerp(0, _maxBarWidth, (float)_playerData.TodaysCarbs / PlayerData.CARBS_REQUIRED_DAILY);
        var carbsSizeDelta = _carbsBar.rectTransform.sizeDelta;
        carbsSizeDelta.x = carbsWidth;
        _carbsBar.rectTransform.sizeDelta = carbsSizeDelta;

        float nutrientsWidth = Mathf.Lerp(0, _maxBarWidth, (float)_playerData.TodaysNutrients / PlayerData.NURTRIENTS_REQUIRD_DAILY);
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
