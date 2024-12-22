using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// TODO change outline highlight color when the bird has been caught before
// TODO mark a new capture, mark already captured, etc

public class BirdingGame : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private float _gameDuration = 5f;
    [SerializeField] private float _beamRotationSpeedDegreesPerSecond = 150f;

    [Header("Trigger")]
    [SerializeField] private List<Sprite> _triggerAnimationFrames = new();
    [SerializeField] private PolygonCollider2D _minTriggerCollider;
    [SerializeField] private PolygonCollider2D _maxTriggerCollider;

    private static BirdingGame _instance;
    public static BirdingGame Instance {
        get {
            if (_instance == null)
                Debug.LogError("Birding game object does not exist");
            return _instance;
        }
    }
    private float _gameTimeElapsed;
    private int _currentTriggerFrameIndex;
    private Vector2[] _minColliderVertices;
    private Vector2[] _maxColliderVertices;
    private float _triggerFrameChangeInterval;
    private float _timeSinceLastTriggerFrameChange;
    private bool _gameOver = false;

    // Points below found by aligning things in Unity 
    private Vector2 _triggerStartPoint = new Vector2(0.53125f, 0f);
    private Vector2 _triggerEndPoint = new Vector2(5.257f, 0f);

    private Vector2 _motionInput = Vector2.zero;
    private Transform _beam;
    private BirdingWinFrame _winFrame;
    private Transform _trigger;
    private PolygonCollider2D _triggerCollider;
    private SpriteRenderer _triggerSpriteRenderer;

    void Awake()
    {
        _instance = this;
        _winFrame = transform.GetChild(0).GetComponent<BirdingWinFrame>();
        _beam = transform.GetChild(1);
        _trigger = _beam.GetChild(0);
        _triggerSpriteRenderer = _trigger.GetComponent<SpriteRenderer>();
        _triggerCollider = _trigger.GetComponent<PolygonCollider2D>();

        InitializeTriggerCollider();
        _triggerFrameChangeInterval = _gameDuration / (_triggerAnimationFrames.Count + 1);
    }

    private void FixedUpdate()
    {
        if (_gameOver)
        {
            if (!_winFrame.gameObject.activeSelf)
                StartCoroutine(EndGame());
            return;
        }

        _gameTimeElapsed += Time.fixedDeltaTime;
        if (_gameTimeElapsed >= _gameDuration)
        {
            OnLose();
            return;
        }

        _timeSinceLastTriggerFrameChange += Time.fixedDeltaTime;
        if (_timeSinceLastTriggerFrameChange >= _triggerFrameChangeInterval)
        {
            _timeSinceLastTriggerFrameChange = 0;
            AdvanceToNextTriggerFrame();
            InterpolateTriggerColliderSize();
        }

        RotateBeam();
        UpdateTriggerPosition();
    }

    public void Play()
    {
        Debug.Log("Birding Game begun");
        _gameOver = false;
        gameObject.SetActive(true);
        _beam.gameObject.SetActive(true);
        _motionInput = Vector2.zero;
        _gameTimeElapsed = 0;
        ResetTrigger();
        AlignBeamToFacingDirection();
        _triggerSpriteRenderer.sprite = _triggerAnimationFrames[_currentTriggerFrameIndex];
    }


    private void AdvanceToNextTriggerFrame()
    {
        _currentTriggerFrameIndex++;
        if (_currentTriggerFrameIndex < _triggerAnimationFrames.Count)
            _triggerSpriteRenderer.sprite = _triggerAnimationFrames[_currentTriggerFrameIndex];
    }
    private void InterpolateTriggerColliderSize()
    {
        Vector2[] _updatedPoints = new Vector2[_triggerCollider.points.Length];

        for (int i = 0; i < _triggerCollider.points.Length; i++)
        {
            _updatedPoints[i] = new Vector2
            (
                Mathf.Lerp
                (
                    _minColliderVertices[i].x,
                    _maxColliderVertices[i].x,
                    _gameTimeElapsed / _gameDuration
                ),
                Mathf.Lerp
                (
                    _minColliderVertices[i].y,
                    _maxColliderVertices[i].y,
                    _gameTimeElapsed / _gameDuration
                )
            );
        }

        // Assign the updated points array back to the PolygonCollider2D
        _triggerCollider.points = _updatedPoints;
    }

    private void RotateBeam()
    {
        if (_motionInput == Vector2.zero)
            return;

        float _maxDelta = Time.fixedDeltaTime * _beamRotationSpeedDegreesPerSecond;
        float _targetAngle = Mathf.Atan2(_motionInput.y, _motionInput.x) * Mathf.Rad2Deg;
        float _delta = Mathf.DeltaAngle(_beam.localEulerAngles.z, _targetAngle);
        _delta = Mathf.Clamp(_delta, -_maxDelta, _maxDelta);

        _beam.localEulerAngles = new Vector3
        (
            _beam.localEulerAngles.x,
            _beam.localEulerAngles.y,
            _beam.localEulerAngles.z + _delta
        );
    }


    private void ResetTrigger()
    {
        _timeSinceLastTriggerFrameChange = 0;
        _currentTriggerFrameIndex = 0;
        _trigger.localPosition = _triggerStartPoint;
        _triggerSpriteRenderer.sprite = _triggerAnimationFrames[_currentTriggerFrameIndex];
        _triggerCollider.points = _maxColliderVertices;
    }

    private void InitializeTriggerCollider()
    {
        _maxColliderVertices = _minTriggerCollider.points;
        _minColliderVertices = _maxTriggerCollider.points;
        if
        (
            _maxColliderVertices.Length != _minColliderVertices.Length ||
            _maxColliderVertices.Length != _triggerCollider.points.Length
        )
        {
            Debug.LogError("The number of verticies of the polygon colliders don't match.");
        }
    }

    private void UpdateTriggerPosition()
    {
        float _newXPosition = Mathf.Lerp(_triggerStartPoint.x, _triggerEndPoint.x, _gameTimeElapsed / _gameDuration);
        _trigger.localPosition = new Vector2
        (
            _newXPosition,
            _trigger.localPosition.y
        );
    }

    private void AlignBeamToFacingDirection()
    {
        _beam.localEulerAngles = new Vector3
        (
            _beam.localEulerAngles.x,
            _beam.localEulerAngles.y,
            PlayerMovementController.Instance.FacingDirection.Value switch
            {
                FacingDirection.East => 0f,
                FacingDirection.North => 90f,
                FacingDirection.West => 180f,
                FacingDirection.South => 270f,
                _ => _beam.localEulerAngles.z
            }
        );
    }

    private void OnMove(InputValue value)
    {
        _motionInput = value.Get<Vector2>();
    }

    private void OnUseTool()
    {
        if (_gameOver) return;

        List<Collider2D> _results = new List<Collider2D>();
        _triggerCollider.OverlapCollider(new ContactFilter2D().NoFilter(), _results);
        List<Bird> _overlappedBirds = new();
        foreach (var _collider in _results)
        {
            Bird _bird = _collider.GetComponent<Bird>();
            if (_bird != null)
            {
                _overlappedBirds.Add(_bird);
            }
        }

        Debug.Log($"Number of overlapping birds: {_overlappedBirds.Count}");
        if (_overlappedBirds.Count == 0)
            OnLose();
        else
            OnWin(_overlappedBirds[0]);
    }

    private void OnWin(Bird winner)
    {
        _gameOver = true;
        _winFrame.transform.position = winner.transform.position;
        _beam.gameObject.SetActive(false);
        _winFrame.PlayWin(winner);
        if (!winner.Caught) {
            winner.Caught = true;
            PlayerData.AddToBirdingLog(winner);
        }
    }

    private void OnLose()
    {
        _gameOver = true;
        StartCoroutine(EndGame());
    }

    private IEnumerator EndGame()
    {
        Debug.Log("Birding Game Ended");
        yield return null;
        gameObject.SetActive(false);
        PlayerMovementController.Instance.PlayerState.Value = PlayerMovementController.PlayerStates.Idle;
    }
}
