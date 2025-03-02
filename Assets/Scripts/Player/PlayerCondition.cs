using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerEnergyManager : MonoBehaviour
{
    [SerializeField] private PlayerData _playerData;
    private PlayerTemperatureManager _playerTemperatureManager;
    private Logger _logger = new();
    private void Awake()
    {
        _playerTemperatureManager = GetComponent<PlayerTemperatureManager>();
    }

    public void Sleep()
    {
        // The player temp is instantly updated to the ambient temperature.
        // The player will not change temp or wetness while asleep.
        //    To explain: If player lights a fire before going to bed 
        //    their sleep quality will improve, and they don't have to stand around waiting
        //    for the player temperature to match ambient.
        // However, the player does have to stand around to dry off. Getting into bed wet should be miserable.
        _playerTemperatureManager.TryUpdatePlayerTempInstantly(true);
        _playerData.SceneOnAwake = SceneManager.GetActiveScene().name;
        LevelChanger.ChangeLevel("SleepMenu");
    }

    public void DepleteEnergy(int energy)
    {
        if (_playerData.CurrentEnergy.Value >= energy)
        {
            _playerData.CurrentEnergy.Value -= energy;
            _logger.Info("Energy depleted by " + energy + ". Current energy: " + _playerData.CurrentEnergy.Value);
        }
        else if (_playerData.CurrentEnergy.Value < energy && _playerData.CurrentEnergy.Value > 0)
        {
            _playerData.CurrentEnergy.Value = 0;
            _logger.Info("Energy insuffucient, this is the last player action");
            _logger.Info("Energy depleted by " + energy + ". Current energy: " + _playerData.CurrentEnergy.Value);
        }
        else
        {
            _logger.Info("No energy left, player cannot perform this action");
        }
    }

    public void RecoverEnergy(int energy)
    {
        if (_playerData.CurrentEnergy.Value + energy <= _playerData.MaxEnergy)
        {
            _playerData.CurrentEnergy.Value += energy;
            _logger.Info("Energy recovered by " + energy + ". Current energy: " + _playerData.CurrentEnergy.Value);
        }
        else
        {
            _playerData.CurrentEnergy.Value = _playerData.MaxEnergy;
            _logger.Info("Energy recovered by " + energy + ". Current energy: " + _playerData.CurrentEnergy.Value);
            _logger.Info("More than energy energy recovered, energy is now at max");
        }
    }

    public bool IsEnergyAvailable()
    {
        return _playerData.CurrentEnergy.Value > 0;
    }
}
