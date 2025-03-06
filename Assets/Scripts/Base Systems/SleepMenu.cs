using System;
using System.Collections;
using UnityEngine;

public class SleepMenu : MonoBehaviour
{
    [SerializeField] private PlayerData _playerData;
    [SerializeField] private float _barTransitionDurationSeconds = 1f;

    [Header("StaminaBar")]
    [SerializeField] private RectTransform _sleepStamina;
    [SerializeField] private RectTransform _proteinStamina;
    [SerializeField] private RectTransform _carbsStamina;
    [SerializeField] private RectTransform _nutrientsStamina;

    [Header("Diet")]
    [SerializeField] private RectTransform _protein;
    [SerializeField] private RectTransform _carbs;
    [SerializeField] private RectTransform _nutrients;
    [SerializeField] private float _dietBarFullWidth;
    [SerializeField] private float _staminaBarFullWidth;
    public static event Action PlayerSlept;

    private void Start()
    {
        InitializeDietBars();
        InitializeStaminaBars();
        StartCoroutine(SleepRoutine());
    }

    private IEnumerator SleepRoutine()
    {
        _playerData.IsPlayerSleeping = true;
        GameClock.Instance.PauseGame();
        
        float _energyFromSleep = _playerData.MaxEnergy * SleepQuality.SLEEP_RECOVERY_RATIO * SleepQuality.GetSleepRecoveryRatio(_playerData.ActualPlayerTemperature.Value);
        float _energyFromDiet = _playerData.MaxEnergy * SleepQuality.DIET_RECOVERY_RATIO * Diet.GetRecoveryRatio(_playerData);
        yield return null; // need to wait for narrator additive scene to load 
        yield return new WaitUntil(() => Narrator.Instance.AreMessagesClear());
        Narrator.Instance.PostMessage(SleepQuality.GetRecoveryMessage(_playerData.ActualPlayerTemperature.Value));
        yield return new WaitForSecondsRealtime(5f);
        yield return GrowStaminaBarFromSleep(_energyFromSleep);

        Narrator.Instance.PostMessage(Diet.GetRecoveryMessage(_playerData));
        yield return new WaitForSecondsRealtime(2f);
        yield return ReduceDietBarAndGrowStaminaBar(_protein, _proteinStamina, _playerData.TodaysProtein, PlayerData.PROTEIN_REQUIRED_DAILY); 
        yield return ReduceDietBarAndGrowStaminaBar(_carbs, _carbsStamina, _playerData.TodaysCarbs, PlayerData.CARBS_REQUIRED_DAILY);
        yield return ReduceDietBarAndGrowStaminaBar(_nutrients, _nutrientsStamina, _playerData.TodaysNutrients, PlayerData.NUTRIENTS_REQUIRED_DAILY);

        yield return new WaitUntil(() => Narrator.Instance.AreMessagesClear());
        yield return new WaitForSecondsRealtime(3f);

        Diet.ResetDailyIntake(_playerData);
        _playerData.CurrentEnergy.Value = Mathf.RoundToInt(_energyFromDiet + _energyFromSleep);

        PlayerSlept?.Invoke();
        GameClock.Instance.SkipToTime(GameClock.Instance.GameDay + 1, SleepQuality.GetAwakeHour(_playerData.ActualPlayerTemperature.Value), 0);
        GameClock.Instance.ResumeGame();
        _playerData.IsPlayerSleeping = false;
        LevelChanger.ChangeLevel(_playerData.SceneOnAwake);
    }

    private void InitializeDietBars()
    {
        SetBarWidth(_protein, _playerData.TodaysProtein, PlayerData.PROTEIN_REQUIRED_DAILY);
        SetBarWidth(_carbs, _playerData.TodaysCarbs, PlayerData.CARBS_REQUIRED_DAILY);
        SetBarWidth(_nutrients, _playerData.TodaysNutrients, PlayerData.NUTRIENTS_REQUIRED_DAILY);
    }

    private void InitializeStaminaBars()
    {
        SetBarWidth(_sleepStamina, 0, 1);
        SetBarWidth(_proteinStamina, 0, 1);
        SetBarWidth(_carbsStamina, 0, 1);
        SetBarWidth(_nutrientsStamina, 0, 1);
    }

    private void SetBarWidth(RectTransform bar, float currentValue, float requiredValue)
    {
        float width = Mathf.Clamp((currentValue / requiredValue) * _dietBarFullWidth, 0, _dietBarFullWidth);
        bar.sizeDelta = new Vector2(width, bar.sizeDelta.y);
    }

    private IEnumerator ReduceDietBarAndGrowStaminaBar(RectTransform dietBar, RectTransform staminaBar, float todaysValue, float requiredValue)
    {
        float elapsedTime = 0f;
        float initialDietWidth = dietBar.sizeDelta.x;
        float targetStaminaWidth = (1f / 3f) * SleepQuality.DIET_RECOVERY_RATIO * (todaysValue / requiredValue) * _staminaBarFullWidth;

        while (elapsedTime < _barTransitionDurationSeconds)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = elapsedTime / _barTransitionDurationSeconds;

            dietBar.sizeDelta = new Vector2(Mathf.Lerp(initialDietWidth, 0, t), dietBar.sizeDelta.y);
            staminaBar.sizeDelta = new Vector2(Mathf.Lerp(0, targetStaminaWidth, t), staminaBar.sizeDelta.y);

            yield return null;
        }

        dietBar.sizeDelta = new Vector2(0, dietBar.sizeDelta.y);
        staminaBar.sizeDelta = new Vector2(targetStaminaWidth, staminaBar.sizeDelta.y);
    }

    private IEnumerator GrowStaminaBarFromSleep(float energyFromSleep)
    {
        float elapsedTime = 0f;
        float targetWidth = energyFromSleep / _playerData.MaxEnergy * _staminaBarFullWidth;

        while (elapsedTime < _barTransitionDurationSeconds)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = elapsedTime / _barTransitionDurationSeconds;

            _sleepStamina.sizeDelta = new Vector2(Mathf.Lerp(0, targetWidth, t), _sleepStamina.sizeDelta.y);

            yield return null;
        }

        _sleepStamina.sizeDelta = new Vector2(targetWidth, _sleepStamina.sizeDelta.y);
    }
}
