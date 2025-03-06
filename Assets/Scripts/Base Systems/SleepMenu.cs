using System;
using System.Collections;
using UnityEngine;

public class SleepMenu : MonoBehaviour
{
    [SerializeField] private PlayerData _playerData;
    public static event Action PlayerSlept;

    private void Start()
    {
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
        Narrator.Instance.PostMessage(Diet.GetRecoveryMessage(_playerData));

        yield return new WaitUntil(() => Narrator.Instance.AreMessagesClear());
        yield return new WaitForSecondsRealtime(2f);

        Diet.ResetDailyIntake(_playerData);
        _playerData.CurrentEnergy.Value = Mathf.RoundToInt(_energyFromDiet + _energyFromSleep);

        PlayerSlept?.Invoke();
        GameClock.Instance.SkipToTime(GameClock.Instance.GameDay + 1, SleepQuality.GetAwakeHour(_playerData.ActualPlayerTemperature.Value), 0);
        GameClock.Instance.ResumeGame();
        _playerData.IsPlayerSleeping = false;
        LevelChanger.ChangeLevel(_playerData.SceneOnAwake);
    }
}
