using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bed : MonoBehaviour, IInteractable
{
    public bool CursorInteract(Vector3 cursorLocation)
    {
        PlayerCondition.Instance.Sleep();
        return true;
    }
}
