using UnityEngine;

public class AbandonedShedInterior : MonoBehaviour
{
    [SerializeField] private AbandonedShedData _shedData;
    [SerializeField] private Sprite _floorRepaired;
    [SerializeField] private GameObject _puddle;
    [SerializeField] private GameObject _rain;
    void Start()
    {
        bool _isRoofRepaired = _shedData.NamesOfRepaired.Contains("roof");
        bool _isFloorRepaired = _shedData.NamesOfRepaired.Contains("floor");

        if (_isRoofRepaired)
        {
             _puddle.SetActive(false);
             _rain.SetActive(false);
        }
        
        if (_isFloorRepaired)
        {
            GetComponent<SpriteRenderer>().sprite = _floorRepaired;
        }
    }
}