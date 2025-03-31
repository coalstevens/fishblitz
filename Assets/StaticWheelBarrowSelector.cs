using UnityEngine;
using UnityEngine.Assertions;

public class StaticWheelBarrowSelector : MonoBehaviour
{
    [SerializeField] public FacingDirection FacingDirection;
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

    public void SetFacingDirection(FacingDirection direction)
    {
        SetDirection(direction);
    }

    private void SetDirection(FacingDirection direction)
    {
        FacingDirection = direction;
        _facingNorth.SetActive(direction == FacingDirection.North);
        _facingEast.SetActive(direction == FacingDirection.East);
        _facingSouth.SetActive(direction == FacingDirection.South);
        _facingWest.SetActive(direction == FacingDirection.West);
    }
}
