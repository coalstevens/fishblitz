using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

// NOTE ---------------
// Methods marked KEEP are to keep functionality for the old method of using the joystick/arrow keys
// for controller the rotation of the birding UI. The active configuration is for mouse tracking.

public class BirdingGame : MonoBehaviour
{
    private enum GameStates { CHARGING, TARGETING }

    [Header("General")]
    [SerializeField] private AudioClip _missedSound; // tag sounds are bird species specific 
    [SerializeField] private float _missedSoundVolume = 1f;
    [SerializeField] private Transform _cursor;
    [SerializeField] private Vector2 _cursorPositionLimits = new Vector2(0f, 1f);
    [SerializeField] private Transform _frame;
    [SerializeField] private Vector2 _framePositionLimits = new Vector2(0f, 1f);
    [SerializeField] private float _bounceOmega = 20f;
    [SerializeField] private Logger _logger = new();

    private static BirdingGame _instance;
    public static BirdingGame Instance
    {
        get
        {
            if (_instance == null)
                Debug.LogError("Birding game object does not exist");
            return _instance;
        }
    }
    private GameStates _gameState = GameStates.CHARGING;
    private Vector2 _motionInput = Vector2.zero;
    private PlayerInput _playerInput;
    private InputAction _useAction;
    private SpriteRenderer _cursorSpriteRenderer;
    private Collider2D cursorCollider;
    private float _angularVelocity = 0f;
    private float _chargeVelocity;
    private float _cursorVelocity;
    private float _frameStopPoint;
    private float _hudRotationSpeedDegreesPerSecond;
    private float _hudAngularAcceleration;
    private bool _bounceComplete = false;
    private float _adjustedCursorEndLimit = 0f;
    private bool _ignoreFirstHandleState = false;
    private bool _useActionReleasedAfterCharge = false; // Requirement for action release after charging state so targeting state isn't immediately ended

    public void Play(Binoculars activeBinoculars)
    {
        ResetGameState();
        _logger.Info("Birding Game begun");
        ConfigureControls();
        GetParametersFromBinoculars(activeBinoculars);
        AlignHUDToMousePosition();
        // SetHUDInitialRotation(); // KEEP
        gameObject.SetActive(true);
    }

    private void Awake()
    {
        _instance = this;
        _cursorSpriteRenderer = _cursor.GetComponent<SpriteRenderer>();
        cursorCollider = _cursor.GetComponent<Collider2D>();

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            _playerInput = player.GetComponent<PlayerInput>();

        Assert.IsNotNull(_playerInput, "PlayerInput component is not set on the player object.");
        Assert.IsNotNull(_cursorSpriteRenderer, "Cursor sprite renderer is not set.");
        Assert.IsTrue(_bounceOmega > 0f, "Frame bounce acceleration must be greater than zero.");

        // Disable on startup if enabled in the editor
        if (gameObject.activeSelf == true)
        {
            gameObject.SetActive(false);
        }
    }

    private void HandleState()
    {
        switch (_gameState)
        {
            case GameStates.CHARGING:
                if (_useAction.IsPressed())
                {
                    _frame.localPosition = new Vector3(_frame.localPosition.x + _chargeVelocity * Time.fixedDeltaTime, 0f, 0f);
                    if (_frame.localPosition.x > _framePositionLimits.y)
                    {
                        _useActionReleasedAfterCharge = false;
                        EnterTargetingState();
                    }
                }
                else
                {
                    _useActionReleasedAfterCharge = true;
                    EnterTargetingState();
                }
                break;
            case GameStates.TARGETING:
                BounceFrame();
                MoveCursor();
                if (_useAction.IsPressed() && _useActionReleasedAfterCharge)
                {
                    TagAnyBirdsUnderCursor();
                    StartCoroutine(EndGame());
                }
                else if (!_useAction.IsPressed())
                {
                    _useActionReleasedAfterCharge = true;
                }

                if (_cursor.localPosition.x >= _adjustedCursorEndLimit)
                {
                    _cursor.localPosition = new Vector3(_adjustedCursorEndLimit, 0f, 0f);
                    StartCoroutine(EndGame());
                    break;
                }
                break;
            default:
                Debug.LogError("Unknown birding game state.");
                break;
        }
    }

    private void TagAnyBirdsUnderCursor()
    {
        _logger.Info("Tag attempted.");
        List<Collider2D> hitColliders = new();
        int numHits = cursorCollider.Overlap(new ContactFilter2D(), hitColliders);

        if (numHits == 0)
        {
            AudioManager.Instance.PlaySFX(_missedSound, _missedSoundVolume);
        }
        else
        {
            foreach (var hit in hitColliders)
            {
                BirdBrain bird = hit.GetComponent<BirdBrain>();
                if (bird != null && !bird.InstanceData.IsTagged.Value)
                {
                    bird.InstanceData.Tag(bird.SpeciesData);
                    _logger.Info($"Tagged bird: {bird.SpeciesData.SpeciesName}");
                    break; // Exit after tagging the first bird
                }
            }
        }
    }

    private void MoveCursor()
    {
        _cursor.localPosition = new Vector3(_cursor.localPosition.x + _cursorVelocity * Time.fixedDeltaTime, 0f, 0f);
    }

    private void BounceFrame()
    {
        if (!_bounceComplete)
        {
            (float newPosition, float newVelocity) = GetNextBounceMotionValues(
                _frame.localPosition.x,
                _chargeVelocity,
                _frameStopPoint,
                _bounceOmega,
                Time.fixedDeltaTime
            );

            _chargeVelocity = newVelocity;
            _frame.localPosition = new Vector3(newPosition, 0f, 0f);

            if (Mathf.Abs(_chargeVelocity) < 0.05f && Mathf.Abs(_frame.localPosition.x - _frameStopPoint) < 0.05f)
            {
                _frame.localPosition = new Vector3(_frameStopPoint, 0f, 0f);
                _bounceComplete = true;
            }
        }
    }

    (float x, float v) GetNextBounceMotionValues(float x, float v, float x_target, float omega, float dt)
    {
        // (Critically damped spring toward x_target)
        // Critical => zeta = 1, c = 2 * sqrt(k * m)
        // Setting m = 1 therefore k = omega^2, we have c = 2 * omega
        // Solve: x'' + 2ωx' + ω²(x - x_target) = 0

        float f = 1.0f + 2.0f * dt * omega;
        float omegaSq = omega * omega;
        float dt_omegaSq = dt * omegaSq;
        float dt2_omegaSq = dt * dt_omegaSq;
        float invDet = 1.0f / (f + dt2_omegaSq);

        float detX = f * x + dt * v + dt2_omegaSq * x_target;
        float detV = v + dt_omegaSq * (x_target - x);

        float newX = detX * invDet;
        float newV = detV * invDet;

        return (newX, newV);
    }

    private void EnterTargetingState()
    {
        _cursorSpriteRenderer.enabled = true;
        _frameStopPoint = _frame.localPosition.x;
        if (_frameStopPoint > _framePositionLimits.y)
            _frameStopPoint = _framePositionLimits.y;

        float _chargeCompletion = _frameStopPoint / (_framePositionLimits.y - _framePositionLimits.x);
        _adjustedCursorEndLimit = _chargeCompletion * (_cursorPositionLimits.y - _cursorPositionLimits.x) + _cursorPositionLimits.x;
        _gameState = GameStates.TARGETING;
    }

    private void FixedUpdate()
    {
        AlignHUDToMousePosition();
        // RotateHUD(); // KEEP

        // This ignore fixes a bug with the first input being misread.
        // I suspect this arises due to the action map switch
        if (_ignoreFirstHandleState)
        {
            _ignoreFirstHandleState = false;
            return;
        }

        HandleState();
    }

    private void AlignHUDToMousePosition()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 dir = mouseWorld - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.localEulerAngles = new Vector3(0f, 0f, angle);
    }

    private void ConfigureControls()
    {
        _playerInput.SwitchCurrentActionMap("Birding");
        _useAction = _playerInput.actions["UseBinoculars"];
        Assert.IsNotNull(_useAction, "Use action is not set.");
    }

    private void ResetGameState()
    {
        _cursorSpriteRenderer.enabled = false;
        _bounceComplete = false;
        _ignoreFirstHandleState = true;
        _useActionReleasedAfterCharge = false;
        _gameState = GameStates.CHARGING;
        _frame.localPosition = new Vector3(_framePositionLimits.x, 0f, 0f); // Reset frame position
        _cursor.localPosition = new Vector3(_cursorPositionLimits.x, 0f, 0f); // Reset cursor position
    }

    private void GetParametersFromBinoculars(Binoculars binoculars)
    {
        _chargeVelocity = (_framePositionLimits.y - _framePositionLimits.x) / binoculars.ChargeTimeSecs;
        _cursorVelocity = (_cursorPositionLimits.y - _cursorPositionLimits.x) / binoculars.CursorTimeSecs;
        // KEEP
        // _hudAngularAcceleration = binoculars.BeamAcceleration;
        // _hudRotationSpeedDegreesPerSecond = binoculars.BeamRotationSpeedDegreesPerSecond;
    }

    // KEEP
    private void SetHUDInitialRotation()
    {
        // Set HUD start rotation
        _motionInput = PlayerMovementController.Instance.CurrentMotion;
        if (_motionInput != Vector2.zero)
        {
            AlignHUDToMotionDirection();
        }
        else
        {
            AlignHUDToFacingDirection();
        }
    }

    // KEEP
    private void RotateHUD()
    {
        if (_motionInput == Vector2.zero)
        {
            // Smooth deceleration when no input is given
            _angularVelocity = Mathf.MoveTowards(_angularVelocity, 0, _hudAngularAcceleration * Time.fixedDeltaTime);
            transform.localEulerAngles += new Vector3(0, 0, _angularVelocity * Time.fixedDeltaTime);
            return;
        }

        // Calculate the target angle from input direction
        float _targetAngle = Mathf.Atan2(_motionInput.y, _motionInput.x) * Mathf.Rad2Deg;
        float _delta = Mathf.DeltaAngle(transform.localEulerAngles.z, _targetAngle);
        float _maxDelta = _hudRotationSpeedDegreesPerSecond;

        // If the difference between the current angle and target angle is very small, snap to target angle
        if (Mathf.Abs(_delta) < 1f) // Threshold for snapping
        {
            _angularVelocity = 0;
            transform.localEulerAngles = new Vector3(0, 0, _targetAngle);  // Snap to target angle
        }
        else
        {
            // Smooth rotation towards target angle
            _angularVelocity = Mathf.MoveTowards(_angularVelocity, Mathf.Sign(_delta) * _maxDelta, _hudAngularAcceleration * Time.fixedDeltaTime);
            transform.localEulerAngles += new Vector3(0, 0, _angularVelocity * Time.fixedDeltaTime);
        }
    }

    // KEEP
    private void AlignHUDToMotionDirection()
    {
        float angle = Mathf.Atan2(_motionInput.y, _motionInput.x) * Mathf.Rad2Deg;
        transform.localEulerAngles = new Vector3
        (
            transform.localEulerAngles.x,
            transform.localEulerAngles.y,
            angle
        );
    }

    // KEEP
    private void AlignHUDToFacingDirection()
    {
        transform.localEulerAngles = new Vector3
        (
            transform.localEulerAngles.x,
            transform.localEulerAngles.y,
            PlayerMovementController.Instance.FacingDirection.Value switch
            {
                CompassDirection.East => 0f,
                CompassDirection.North => 90f,
                CompassDirection.West => 180f,
                CompassDirection.South => 270f,
                _ => transform.localEulerAngles.z
            }
        );
    }

    // KEEP
    // private void OnRotate(InputValue value)
    // {
    //     _motionInput = value.Get<Vector2>();
    // }

    private IEnumerator EndGame()
    {
        _logger.Info("Birding Game Complete");
        _angularVelocity = 0;
        _playerInput.SwitchCurrentActionMap("Player");
        yield return null;
        gameObject.SetActive(false);
        PlayerMovementController.Instance.PlayerState.Value = PlayerMovementController.PlayerStates.Idle;
    }
}
