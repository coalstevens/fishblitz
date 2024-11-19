using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BirdingGame : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private float _gameDuration = 5f;
    [SerializeField] private float _beamRotationSpeedDegreesPerSecond = 150f;

    [Header("Trigger")]
    [SerializeField] private List<Sprite> _triggerAnimationFrames = new();
    [SerializeField] private PolygonCollider2D _minTriggerCollider;
    [SerializeField] private PolygonCollider2D _maxTriggerCollider;

    private float _gameTimeElapsed;
    private int _currentTriggerFrameIndex;
    private Vector2[] _minColliderVertices;
    private Vector2[] _maxColliderVertices;
    private float _triggerFrameChangeInterval;
    private float _timeSinceLastTriggerFrameChange;

    // Points below found by aligning things in Unity 
    private Vector2 _triggerStartPoint = new Vector2(0.53125f, -0.46875f);
    private Vector2 _triggerEndPoint = new Vector2(5.257f, 0f);

    private PlayerMovementController _playerMovementController;
    private Vector2 _motionInput = Vector2.zero;
    private Transform _beam;
    private Transform _trigger;
    private PolygonCollider2D _triggerCollider;
    private SpriteRenderer _triggerSpriteRenderer;

    void Awake()
    {
        _playerMovementController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovementController>();
        _beam = transform.GetChild(0);
        _trigger = _beam.GetChild(0);
        _triggerSpriteRenderer = _trigger.GetComponent<SpriteRenderer>();
        _triggerCollider = _trigger.GetComponent<PolygonCollider2D>();

        InitializeTriggerCollider();
        _triggerFrameChangeInterval = _gameDuration / (_triggerAnimationFrames.Count + 1);
    }

    private void FixedUpdate()
    {
        _gameTimeElapsed += Time.fixedDeltaTime;
        _timeSinceLastTriggerFrameChange += Time.fixedDeltaTime;

        if (_gameTimeElapsed >= _gameDuration)
        {
            Stop();
            return;
        }

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
        gameObject.SetActive(true);
        _motionInput = Vector2.zero;
        _gameTimeElapsed = 0;
        ResetTrigger();
        AlignBeamToFacingDirection();
        _triggerSpriteRenderer.sprite = _triggerAnimationFrames[_currentTriggerFrameIndex];
    }

    public void Stop()
    {
        gameObject.SetActive(false);
    }

    private void AdvanceToNextTriggerFrame()
    {
        _currentTriggerFrameIndex++;
        if (_currentTriggerFrameIndex < _triggerAnimationFrames.Count)
            _triggerSpriteRenderer.sprite = _triggerAnimationFrames[_currentTriggerFrameIndex];
    }

    private void InterpolateTriggerColliderSize()
    {
        for (int i = 0; i < _triggerCollider.points.Length; i++)
        {
            _triggerCollider.points[i].x = Mathf.Lerp
            (
                _maxColliderVertices[i].x,
                _minColliderVertices[i].x,
                _gameTimeElapsed / _gameDuration
            );

            _triggerCollider.points[i].y = Mathf.Lerp
            (
                _maxColliderVertices[i].y,
                _minColliderVertices[i].y,
                _gameTimeElapsed / _gameDuration
            );
        }
    }

    private void RotateBeam()
    {
        _beam.localEulerAngles = new Vector3
        (
            _beam.localEulerAngles.x,
            _beam.localEulerAngles.y,
            _beam.localEulerAngles.z + (_motionInput.x * Time.fixedDeltaTime * _beamRotationSpeedDegreesPerSecond)
        );
    }


    private void ResetTrigger()
    {
        _timeSinceLastTriggerFrameChange = 0;
        _currentTriggerFrameIndex = 0;
        if (_trigger == null)
            Debug.Log("trigger");
        if (_triggerStartPoint == null)
            Debug.Log("startpoints");
        _trigger.localPosition= _triggerStartPoint;
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
            _playerMovementController.FacingDirection.Value switch
            {
                FacingDirection.East => 0f,
                FacingDirection.North => 90f,
                FacingDirection.West => 180f,
                FacingDirection.South => 270f,
                _ => _beam.localEulerAngles.z
            }
        );
    }

    public void OnMove(InputValue value)
    {
        if (_playerMovementController.PlayerState.Value == PlayerStates.Birding)
            _motionInput = value.Get<Vector2>();
    }
}
