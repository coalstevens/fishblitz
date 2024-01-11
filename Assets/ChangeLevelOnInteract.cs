using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using OysterUtils;

public class ChangeLevelOnInteract : MonoBehaviour, IInteractableWorldObject
{
    [SerializeField] private string _toScene;
    [SerializeField] private Vector3 _sceneSpawnLocation;

    public bool CursorAction(TileData tileData, Vector3 cursorLocation)
    {
        SmoothSceneManager.LoadScene(_toScene);
        PlayerData.Instance.SceneSpawnPosition = _sceneSpawnLocation;
        return true;
    }
}
