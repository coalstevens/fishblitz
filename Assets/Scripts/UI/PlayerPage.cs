using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPage : MonoBehaviour, GameMenuManager.IGameMenuPage
{
    public void DisableCursor()
    {
        // do nothing
    }

    public void EnableCursor()
    {
        // do nothing
    }

    public void LoadPage()
    {
        gameObject.SetActive(true);
    }

    public void UnloadPage()
    {
        gameObject.SetActive(false);
    }
    
    public bool MoveCursor(Vector2 inputDirection)
    {
        return false;
    }

    public void Select()
    {
        // do nothing
    }

}
