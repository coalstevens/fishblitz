using UnityEngine;

public class AbandonedShedInterior : MonoBehaviour
{
    [SerializeField] private AbandonedShedData _shedData;
    [SerializeField] private Sprite _floorRepaired;
    [SerializeField] private GameObject _puddle;
    [SerializeField] private GameObject _roofLight;
    [SerializeField] private GameObject _rainParticleSystem;
    void Start()
    {
        bool _isNotRaining = WorldState.RainState.Value == WorldState.RainStates.NoRain;
        bool _isRoofRepaired = _shedData.NamesOfRepaired.Contains("roof");
        bool _isFloorRepaired = _shedData.NamesOfRepaired.Contains("floor");

        if (_isRoofRepaired)
            _roofLight.SetActive(false);
        
        if (_isRoofRepaired || _isNotRaining)
        {
            _rainParticleSystem.SetActive(false);
            _puddle.SetActive(false);
        }

        if (_isFloorRepaired)
        {
            GetComponent<SpriteRenderer>().sprite = _floorRepaired;
        }
    }
}