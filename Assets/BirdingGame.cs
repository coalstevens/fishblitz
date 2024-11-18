using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class BirdingGame : MonoBehaviour
{
    [SerializeField] private float _beamRotationSpeed = 2f;
    [SerializeField] private float _triggerSpeed = 2f;
    [SerializeField] private Transform _trailingLine;
    [SerializeField] private Transform _middleLine;
    [SerializeField] private Transform _leadingLine;

    // Trigger
    private Vector2 _triggerStartPosition = new Vector2(1.4103f, 0.932f); // Values found experimentally
    private Vector2 _triggerEndPosition = new Vector2(6.7228f, 0.932f);
    private List<(int position, int height)> _beamDimensionsPixels = new() { // Measured from drawing
        (0, 2),
        (1, 4),
        (5, 6),
        (20, 8),
        (34, 10),
        (48, 12),
        (62, 14),
        (76, 16),
        (90, 18),
        (104, 20),
        (118, 22),
        (132, 24),
        (146, 26),
        (160, 28),
    };

    private float _pixelSize = 1f/32f;
    
    private List<(Transform line, int index)> _triggerLines = new();

    private PlayerMovementController _playerMovementController;
    private Vector2 _motionInput = Vector2.zero;
    private Transform _beam;
    void Awake()
    {
        _playerMovementController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovementController>();
        _beam = transform.GetChild(0);

        _triggerLines.Add((_trailingLine, 0));
        _triggerLines.Add((_middleLine, 0));
        _triggerLines.Add((_leadingLine, 0));
    }
    
    void ResetTriggerLineIndices() {
        for(int i = 0; i < _triggerLines.Count; i++)
            _triggerLines[i] = new (_triggerLines[i].line, 0);
    }

    private void FixedUpdate()
    {
        RotateBeam();
        UpdateTriggerLinePosition();
        UpdateTriggerLinesScaling();
    }

    private void RotateBeam() {
        _beam.localEulerAngles = new Vector3
        (
            _beam.localEulerAngles.x,
            _beam.localEulerAngles.y,
            _beam.localEulerAngles.z + (_motionInput.x * Time.fixedDeltaTime * _beamRotationSpeed)
        );
    }

    public void Play()
    {
        gameObject.SetActive(true);
        ResetTriggerLineIndices();
        InitializeTriggerLinePositions();
        InitializeTriggerLineScales();
    }

    public void Stop()
    {
        gameObject.SetActive(false);
    }

    private void InitializeTriggerLinePositions()
    {
        float[] _offsets = { -1, 0, 1 }; // Offsets for trailing, middle, leading lines
        for (int i = 0; i < _triggerLines.Count; i++)
        {
            _triggerLines[i].line.localPosition = new Vector2
            (
                _triggerStartPosition.x + (_offsets[i] * _pixelSize),
                _triggerStartPosition.y
            );
        }
    }
    
    private void InitializeTriggerLineScales() 
    {
        for(int i = 0; i <_triggerLines.Count; i++)
            _triggerLines[i].line.localScale = new Vector2 (1,1);
    }

    private void UpdateTriggerLinesScaling() {
        for(int i = 0; i < _triggerLines.Count; i++) {
            if (_triggerLines[i].index >= _beamDimensionsPixels.Count)
                continue;

            if (_triggerLines[i].line.localPosition.x < _triggerStartPosition.x + _beamDimensionsPixels[_triggerLines[i].index].position * _pixelSize)
                continue;

            _triggerLines[i].line.localScale = new Vector2
            (
                _triggerLines[i].line.localScale.x,
                _beamDimensionsPixels[_triggerLines[i].index].height / 2
            );

            _triggerLines[i] = (_triggerLines[i].line, _triggerLines[i].index + 1);
        }
    }

    private void UpdateTriggerLinePosition()
    {
        float _moveDistance = Time.fixedDeltaTime * _triggerSpeed;
        for (int i = 0; i < _triggerLines.Count; i++)
        {
            _triggerLines[i].line.localPosition = new Vector2
            (
                _triggerLines[i].line.localPosition.x + _moveDistance,
                _triggerLines[i].line.localPosition.y
            );
        }
    }

    public void OnMove(InputValue value)
    {
        if (_playerMovementController.PlayerState.Value == PlayerStates.Birding)
            _motionInput = value.Get<Vector2>();
    }
}
