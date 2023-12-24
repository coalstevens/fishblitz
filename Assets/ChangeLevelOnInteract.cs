using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using OysterUtils;

public class ChangeLevelOnInteract : MonoBehaviour, ICursorInteractableObject
{
    [SerializeField] private string _toScene;

    public void CursorAction(TileData tileData, Vector3 cursorLocation)
    {
        SmoothSceneManager.LoadScene(_toScene);
    }
}
