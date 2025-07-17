using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class BirdingGame : MonoBehaviour
{
    private enum GameStates { CHARGING, BOUNCING, CURSOR_MOVING }
    private GameStates _gameState = GameStates.CHARGING;

    [Header("General")]
    [SerializeField] private Transform _cursor;
    [SerializeField] private Vector2 _cursorPositionLimits = new Vector2(0f, 1f);
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
    private float _currentBeamAngularVelocity = 0f;
    private Vector2 _motionInput = Vector2.zero;
    [SerializeField] private Transform _frame;
    [SerializeField] private Vector2 _framePositionLimits = new Vector2(0f, 1f);
    [SerializeField] private float _bounceOmega = 0.5f;
    private float _frameVelocity;
    private PlayerInput _playerInput;
    private InputAction _useAction;
    private Binoculars _activeBinoculars;
    private float _chargeCompletion = 0f;
    private float _frameStopPoint;
    private float _adjustedCursorEndLimit = 0f;
    private float _cursorSpeed;
    private SpriteRenderer _cursorSpriteRenderer;

    private void Awake()
    {
        _instance = this;
        _cursorSpriteRenderer = _cursor.GetComponent<SpriteRenderer>();

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            _playerInput = player.GetComponent<PlayerInput>();

        Assert.IsTrue(_bounceOmega > 0f, "Frame bounce acceleration must be greater than zero.");

        // Disable on startup if enabled in the editor
        if (gameObject.activeSelf == true)
        {
            gameObject.SetActive(false);
        }
    }

    private void HandleState()
    {
        Assert.IsNotNull(_useAction, "Use action is not set.");

        switch (_gameState)
        {
            case GameStates.CHARGING:
                if (_useAction.IsPressed())
                {
                    _frame.localPosition = new Vector3(_frame.localPosition.x + _frameVelocity * Time.fixedDeltaTime, 0f, 0f);
                    if (_frame.localPosition.x > _framePositionLimits.y)
                    {
                        _logger.Info("Charge completed. Transitioning to BOUNCING state.");
                        EnterBouncingState();
                    }
                }
                else
                {
                    EnterBouncingState();
                    _logger.Info("Charge interrupted. Transitioning to BOUNCING state.");
                }
                break;
            case GameStates.BOUNCING:
                Debug.Log(_frameVelocity);
                (float newPosition, float newVelocity) = GetUpdatedCriticallyDampedValues(
                    _frame.localPosition.x,
                    _frameVelocity,
                    _frameStopPoint,
                    _bounceOmega,
                    Time.fixedDeltaTime
                );

                _frameVelocity = newVelocity;
                _frame.localPosition = new Vector3(newPosition, 0f, 0f);

                if (Mathf.Abs(_frameVelocity) < 0.01f && Mathf.Abs(_frame.localPosition.x - _frameStopPoint) < 0.01f)
                {
                    _frame.localPosition = new Vector3(_frameStopPoint, 0f, 0f);
                    EnterCursorMovingState();
                }
                break;
            case GameStates.CURSOR_MOVING:
                _cursor.localPosition = new Vector3(_cursor.localPosition.x + _cursorSpeed * Time.fixedDeltaTime, 0f, 0f);
                if (_cursor.localPosition.x >= _adjustedCursorEndLimit)
                {
                    _cursor.localPosition = new Vector3(_adjustedCursorEndLimit, 0f, 0f);
                    StartCoroutine(EndGame());
                }
                break;
            default:
                Debug.LogError("Unknown birding game state.");
                break;
        }
    }

    (float x, float v) GetUpdatedCriticallyDampedValues(float x, float v, float x_target, float omega, float dt)
    {
        // (Critically damped spring toward x_target)

        // my'' + cy' + ky = 0 where x = (x - x_target)
        // Critically damped mean zeta = 1, c = 2 * sqrt(k * m)
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

    private void EnterBouncingState()
    {
        _frameStopPoint = _frame.localPosition.x;
        if (_frameStopPoint > _framePositionLimits.y)
            _frameStopPoint = _framePositionLimits.y;

        _chargeCompletion = _frameStopPoint / (_framePositionLimits.y - _framePositionLimits.x);
        _adjustedCursorEndLimit = _chargeCompletion * (_cursorPositionLimits.y - _cursorPositionLimits.x) + _cursorPositionLimits.x;
        _gameState = GameStates.BOUNCING;
        _logger.Info("Transitioning to BOUNCING state.");
    }

    private void EnterCursorMovingState()
    {
        _cursorSpeed = (_cursorPositionLimits.y - _cursorPositionLimits.x) / _activeBinoculars.CursorTimeSecs;
        _gameState = GameStates.CURSOR_MOVING;
        _cursorSpriteRenderer.enabled = true;
        _logger.Info("Transitioning to CURSOR_MOVING state.");
    }

    private void FixedUpdate()
    {
        RotateBeam();
        HandleState();
    }

    public void Play(Binoculars activeBinoculars)
    {
        _logger.Info("Birding Game begun");

        // Set up inputs
        _playerInput.SwitchCurrentActionMap("Birding");
        _useAction = _playerInput.actions["UseBinoculars"];
        _activeBinoculars = activeBinoculars;

        // Reset game state
        _cursorSpriteRenderer.enabled = false;
        _gameState = GameStates.CHARGING;

        _frameVelocity = (_framePositionLimits.y - _framePositionLimits.x) / _activeBinoculars.ChargeTimeSecs;
        _frame.localPosition = new Vector3(_framePositionLimits.x, 0f, 0f); // Reset frame position
        _cursor.localPosition = new Vector3(_cursorPositionLimits.x, 0f, 0f); // Reset cursor position

        // Set start position for HUD
        _motionInput = PlayerMovementController.Instance.CurrentMotion;
        if (_motionInput != Vector2.zero)
            AlignHUDToMotionDirection();
        else
            AlignHUDToFacingDirection();

        gameObject.SetActive(true);

        // if (!_useAction.IsPressed())
        // {
        //     StartCoroutine(EndGame());
        // }

    }

    private void RotateBeam()
    {
        if (_motionInput == Vector2.zero)
        {
            // Smooth deceleration when no input is given
            _currentBeamAngularVelocity = Mathf.MoveTowards(_currentBeamAngularVelocity, 0, _activeBinoculars.BeamAcceleration * Time.fixedDeltaTime);
            transform.localEulerAngles += new Vector3(0, 0, _currentBeamAngularVelocity * Time.fixedDeltaTime);
            return;
        }

        // Calculate the target angle from input direction
        float _targetAngle = Mathf.Atan2(_motionInput.y, _motionInput.x) * Mathf.Rad2Deg;
        float _delta = Mathf.DeltaAngle(transform.localEulerAngles.z, _targetAngle);
        float _maxDelta = _activeBinoculars.BeamRotationSpeedDegreesPerSecond;

        // If the difference between the current angle and target angle is very small, snap to target angle
        if (Mathf.Abs(_delta) < 1f) // Threshold for snapping
        {
            _currentBeamAngularVelocity = 0;
            transform.localEulerAngles = new Vector3(0, 0, _targetAngle);  // Snap to target angle
        }
        else
        {
            // Smooth rotation towards target angle
            _currentBeamAngularVelocity = Mathf.MoveTowards(_currentBeamAngularVelocity, Mathf.Sign(_delta) * _maxDelta, _activeBinoculars.BeamAcceleration * Time.fixedDeltaTime);
            transform.localEulerAngles += new Vector3(0, 0, _currentBeamAngularVelocity * Time.fixedDeltaTime);
        }
    }

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

    private void AlignHUDToFacingDirection()
    {
        transform.localEulerAngles = new Vector3
        (
            transform.localEulerAngles.x,
            transform.localEulerAngles.y,
            PlayerMovementController.Instance.FacingDirection.Value switch
            {
                FacingDirection.East => 0f,
                FacingDirection.North => 90f,
                FacingDirection.West => 180f,
                FacingDirection.South => 270f,
                _ => transform.localEulerAngles.z
            }
        );
    }

    private void OnRotate(InputValue value)
    {
        _motionInput = value.Get<Vector2>();
    }

    // private void OnUseBinoculars()
    // {
    //     if (PlayerMovementController.Instance.PlayerState.Value != PlayerMovementController.PlayerStates.Birding)
    //         return;
    // }

    private IEnumerator EndGame()
    {
        _logger.Info("Birding Game Ended");
        _currentBeamAngularVelocity = 0;
        _playerInput.SwitchCurrentActionMap("Player");
        yield return null;
        gameObject.SetActive(false);
        PlayerMovementController.Instance.PlayerState.Value = PlayerMovementController.PlayerStates.Idle;
    }
}
