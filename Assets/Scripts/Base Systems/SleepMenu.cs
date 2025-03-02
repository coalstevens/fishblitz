using System;
using System.Collections;
using UnityEngine;

public class SleepMenu : MonoBehaviour
{
    [SerializeField] PlayerData _playerData;
    public static event Action PlayerSlept;
    private void Start()
    {
        StartCoroutine(SleepRoutine());
    }
    
    private IEnumerator SleepRoutine()
    {
        _playerData.IsPlayerSleeping = true;
        GameClock.Instance.PauseGame();
        GameClock.Instance.SkipToTime(GameClock.Instance.GameDay.Value + 1, SleepQuality.GetAwakeHour(_playerData.ActualPlayerTemperature.Value), 0);
        yield return new WaitForSecondsRealtime(1f);
        NarratorSpeechController.Instance.PostMessage(SleepQuality.GetAwakeMessage(_playerData.ActualPlayerTemperature.Value));
        yield return new WaitUntil(() => NarratorSpeechController.Instance.AreMessagesClear());
        yield return new WaitForSecondsRealtime(1f);

        float _energyFromSleep = SleepQuality.SLEEP_RECOVERY_RATIO * SleepQuality.GetSleepRecoveryRatio(_playerData.ActualPlayerTemperature.Value);
        float _energyFromDiet = SleepQuality.DIET_RECOVERY_RATIO * Diet.GetRecoveryRatio(_playerData);
        _playerData.CurrentEnergy.Value = Mathf.RoundToInt(_energyFromDiet + _energyFromSleep);

        PlayerSlept.Invoke();
        GameClock.Instance.ResumeGame();
        _playerData.IsPlayerSleeping = false;
        LevelChanger.ChangeLevel(_playerData.SceneOnAwake);
    }
}
