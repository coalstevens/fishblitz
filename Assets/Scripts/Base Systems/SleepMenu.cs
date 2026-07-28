using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SleepMenu : MonoBehaviour
{
    private PlayerEnergyManager _energyManager;
    private PlayerTemperatureManager _temperatureManager;
    private PlayerSceneData _sceneData;
    public static event Action PlayerSlept;

    private void Start()
    {
        _energyManager = FindFirstObjectByType<PlayerEnergyManager>();
        _temperatureManager = FindFirstObjectByType<PlayerTemperatureManager>();
        _sceneData = FindFirstObjectByType<PlayerSceneData>();
        StartCoroutine(SleepRoutine());
    }

    private IEnumerator SleepRoutine()
    {
        _energyManager.IsPlayerSleeping = true;
        GameClock.Instance.PauseGame();
        
        float _energyFromSleep = _energyManager.MaxEnergy * SleepQuality.SLEEP_RECOVERY_RATIO * SleepQuality.GetSleepRecoveryRatio(_temperatureManager.ActualPlayerTemperature.Value);
        float _energyFromDiet = _energyManager.MaxEnergy * SleepQuality.DIET_RECOVERY_RATIO * Diet.GetRecoveryRatio(_energyManager);

        yield return null;
        yield return new WaitUntil(() => Narrator.Instance.AreMessagesClear());

        Narrator.Instance.PostMessage(SleepQuality.GetRecoveryMessage(_temperatureManager.ActualPlayerTemperature.Value));
        Narrator.Instance.PostMessage(Diet.GetRecoveryMessage(_energyManager));

        yield return new WaitUntil(() => Narrator.Instance.AreMessagesClear());
        yield return new WaitForSecondsRealtime(2f);

        Diet.ResetDailyIntake(_energyManager);
        _energyManager.CurrentEnergy.Value = Mathf.RoundToInt(_energyFromDiet + _energyFromSleep);

        PlayerSlept?.Invoke();
        GameClock.Instance.SkipToTime(GameClock.Instance.GameDay + 1, SleepQuality.GetAwakeHour(_temperatureManager.ActualPlayerTemperature.Value), 0);
        GameClock.Instance.ResumeGame();
        _energyManager.IsPlayerSleeping = false;
        LevelChanger.ChangeLevel(_sceneData.SceneOnAwake);
    }
}
