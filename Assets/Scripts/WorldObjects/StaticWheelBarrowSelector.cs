using UnityEngine;

public class StaticWheelBarrowSelector : MonoBehaviour
{
    [SerializeField] public CompassDirection FacingDirection;

    private void Start()
    {
        SetDirection();
    }

    public void SetFacingDirection(CompassDirection direction)
    {
        FacingDirection = direction;
        SetDirection();
    }

    private void SetDirection()
    {
        switch (FacingDirection)
        {
            case CompassDirection.SouthWest:
                transform.localScale = new Vector3(-1, 1, 1);
                break;
            case CompassDirection.SouthEast:
                transform.localScale = new Vector3(1, 1, 1);
                break;
            default:
                Debug.LogError("Wheelbarrow direction not handled.");
                break;
        }
    }
}
