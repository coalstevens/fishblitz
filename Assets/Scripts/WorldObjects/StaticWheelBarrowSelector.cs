using UnityEngine;
using UnityEngine.Assertions;

public class StaticWheelBarrowSelector : MonoBehaviour
{
    [SerializeField] public CompassDirection FacingDirection;
    [SerializeField] private GameObject _facingNorth;
    [SerializeField] private GameObject _facingEast;
    [SerializeField] private GameObject _facingSouth;
    [SerializeField] private GameObject _facingWest;

    private void Start()
    {
        Assert.IsNotNull(_facingNorth);
        Assert.IsNotNull(_facingEast);
        Assert.IsNotNull(_facingSouth);
        Assert.IsNotNull(_facingWest);
        
        SetDirection(FacingDirection);
    }

    public void SetFacingDirection(CompassDirection direction)
    {
        SetDirection(direction);
    }

    private void SetDirection(CompassDirection direction)
    {
        FacingDirection = direction;
        _facingNorth.SetActive(direction == CompassDirection.North);
        _facingEast.SetActive(direction == CompassDirection.East);
        _facingSouth.SetActive(direction == CompassDirection.South);
        _facingWest.SetActive(direction == CompassDirection.West);
    }
}
