using Newtonsoft.Json;
using ReactiveUnity;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public class PlayerEnergyManager : MonoBehaviour, ISaveableComponent
{
    public interface IEnergyDepleting
    {
        public int EnergyCost { get; }
    }

    public string ComponentId => "PlayerEnergy";

    [Header("Energy")]
    public Reactive<int> CurrentEnergy = new Reactive<int>(0);
    [SerializeField] private int _maxEnergy = 100;
    public int MaxEnergy => _maxEnergy;

    [Header("Diet")]
    public float TodaysProtein = 0;
    public float TodaysCarbs = 0;
    public float TodaysNutrients = 0;
    public const float PROTEIN_REQUIRED_DAILY = 100;
    public const float CARBS_REQUIRED_DAILY = 100;
    public const float NUTRIENTS_REQUIRED_DAILY = 100;

    [Header("Sleep")]
    public bool IsPlayerSleeping = false;
    public int LastPlayerSleepTime = 0;

    private PlayerTemperatureManager _playerTemperatureManager;
    private PlayerSceneData _playerSceneData;
    private Logger _logger = new();

    [System.Serializable]
    private class State
    {
        public int CurrentEnergy;
        public int MaxEnergy;
        public float TodaysProtein;
        public float TodaysCarbs;
        public float TodaysNutrients;
        public bool IsPlayerSleeping;
        public int LastPlayerSleepTime;
    }

    private void Awake()
    {
        _playerTemperatureManager = GetComponent<PlayerTemperatureManager>();
        _playerSceneData = GetComponent<PlayerSceneData>();
    }

    public void Sleep()
    {
        _playerTemperatureManager.TryUpdatePlayerTempInstantly(true);
        _playerSceneData.SceneOnAwake = SceneManager.GetActiveScene().name;
        LastPlayerSleepTime = GameClock.Instance.GameMinutesElapsed;
        LevelChanger.ChangeLevel("SleepMenu");
    }

    public void DepleteEnergy(int energy)
    {
        Assert.IsTrue(energy >= 0, "Energy to deplete must be non negative.");
        if (CurrentEnergy.Value >= energy)
        {
            CurrentEnergy.Value -= energy;
            _logger.Info("Energy depleted by " + energy + ". Current energy: " + CurrentEnergy.Value);
        }
        else if (CurrentEnergy.Value < energy && CurrentEnergy.Value > 0)
        {
            CurrentEnergy.Value = 0;
            _logger.Info("Energy insuffucient, this is the last player action");
            _logger.Info("Energy depleted by " + energy + ". Current energy: " + CurrentEnergy.Value);
        }
        else
        {
            _logger.Info("No energy left, player cannot perform this action");
        }
    }

    public void RecoverEnergy(int energy)
    {
        Assert.IsTrue(energy >= 0, "Energy to recover must be non negative.");
        if (CurrentEnergy.Value + energy <= MaxEnergy)
        {
            CurrentEnergy.Value += energy;
            _logger.Info("Energy recovered by " + energy + ". Current energy: " + CurrentEnergy.Value);
        }
        else
        {
            CurrentEnergy.Value = MaxEnergy;
            _logger.Info("Energy recovered by " + energy + ". Current energy: " + CurrentEnergy.Value);
            _logger.Info("More than energy energy recovered, energy is now at max");
        }
    }

    public bool IsEnergyAvailable()
    {
        return CurrentEnergy.Value > 0;
    }

    public bool IsSufficientEnergyAvailable(IEnergyDepleting energyDepletingThing)
    {
        if(energyDepletingThing == null)
            return false;
        if (IsEnergyAvailable())
            return true;
        _logger.Info("Not enough energy remaining.");
        return false;
    }

    public string CaptureStateAsJson()
    {
        var _state = new State
        {
            CurrentEnergy = CurrentEnergy.Value,
            MaxEnergy = _maxEnergy,
            TodaysProtein = TodaysProtein,
            TodaysCarbs = TodaysCarbs,
            TodaysNutrients = TodaysNutrients,
            IsPlayerSleeping = IsPlayerSleeping,
            LastPlayerSleepTime = LastPlayerSleepTime
        };
        return JsonConvert.SerializeObject(_state);
    }

    public void RestoreStateFromJson(string json)
    {
        var _state = JsonConvert.DeserializeObject<State>(json);
        CurrentEnergy.Value = _state.CurrentEnergy;
        _maxEnergy = _state.MaxEnergy;
        TodaysProtein = _state.TodaysProtein;
        TodaysCarbs = _state.TodaysCarbs;
        TodaysNutrients = _state.TodaysNutrients;
        IsPlayerSleeping = _state.IsPlayerSleeping;
        LastPlayerSleepTime = _state.LastPlayerSleepTime;
    }

    public void ResetToDefaults()
    {
        CurrentEnergy.Value = _maxEnergy;
        TodaysProtein = 0;
        TodaysCarbs = 0;
        TodaysNutrients = 0;
        IsPlayerSleeping = false;
    }
}
