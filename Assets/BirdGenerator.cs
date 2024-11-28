using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdGenerator : MonoBehaviour
{
    [SerializeField] int _birdsInScene = 15;
    [SerializeField] Collider2D _world;
    [SerializeField] private string _birdDirectioryName = "Birds"; // Replace with your subfolder name
    private Bounds _worldBounds;
    private GameObject[] _birds;

    void Start() 
    {
        _birds = Resources.LoadAll<GameObject>(_birdDirectioryName);
        _worldBounds = _world.bounds;
    }

    void Update()
    {
        if(transform.childCount < _birdsInScene)
            SpawnBird();
    }

    void SpawnBird() 
    {
        if (_birds.Length == 0)
        {
            Debug.LogError("No birds found in the Resources/" + _birdDirectioryName + " folder.");
            return;
        }

        GameObject _randomBird = _birds[Random.Range(0, _birds.Length)];
        
        UnityEngine.Object.Instantiate
        (
            _birds[0],//_randomBird,
            GetPointWithinWorldAndOutsideCamera(),
            Quaternion.identity,
            transform
        );
    }

    private Vector2 GetPointWithinWorldAndOutsideCamera()
    {
        Bounds _cameraBounds = GetCameraFrameBounds();
        Vector2 _randomPoint;

        do
        {
            float _x = Random.Range(_worldBounds.min.x, _worldBounds.max.x);
            float _y = Random.Range(_worldBounds.min.y, _worldBounds.max.y);
            _randomPoint = new Vector2(_x, _y);
        } while (!_cameraBounds.Contains(_randomPoint));

        return _randomPoint;
    }

    private Bounds GetCameraFrameBounds() {
        Camera _mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        float _cameraZPosition = Mathf.Abs(_mainCamera.transform.position.z);

        Vector3 _bottomLeft = _mainCamera.ViewportToWorldPoint(new Vector3(0, 0, _cameraZPosition));
        Vector3 _topRight = _mainCamera.ViewportToWorldPoint(new Vector3(1, 1, _cameraZPosition));

        Bounds _cameraBounds = new Bounds();
        _cameraBounds.SetMinMax(_bottomLeft, _topRight);

        Vector3 _boundsCenter = _cameraBounds.center;
        Vector3 _boundsSize = _cameraBounds.size;
        //Debug.Log($"Center: {_boundsCenter}, Size: {_boundsSize}");

        return _cameraBounds;
    }
}
