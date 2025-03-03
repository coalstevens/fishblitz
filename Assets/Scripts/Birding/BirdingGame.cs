using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class BirdingGame : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private float _beamRotationSpeedDegreesPerSecond = 150f;
    [SerializeField] private float _beamAcceleration = 200f;       
    [SerializeField] private Logger _logger = new();

    private static BirdingGame _instance;
    public static BirdingGame Instance {
        get {
            if (_instance == null)
                Debug.LogError("Birding game object does not exist");
            return _instance;
        }
    }
    private float _currentBeamAngularVelocity = 0f;
    private Vector2 _motionInput = Vector2.zero;
    private Transform _beam;
    private Light2D _beamLight;
    private SunLightControl _sun;
    public Collider2D BeamCollider => _beam.GetComponent<Collider2D>();

    void Start()
    {
        _instance = this;
        _beam = transform.GetChild(0);
        _beamLight = GetComponentInChildren<Light2D>();
        _sun = GameObject.FindGameObjectWithTag("Sun")?.GetComponent<SunLightControl>();
    }

    private void FixedUpdate()
    {
        RotateBeam();
    }

    public void Play()
    {
        _logger.Info("Birding Game begun");
        if (_sun != null) _sun.FadeOutLight(0.3f);
        gameObject.SetActive(true);
        _beam.gameObject.SetActive(true);
        _motionInput = PlayerMovementController.Instance.CurrentMotion;
        if (_motionInput != Vector2.zero)
            AlignBeamToMotionDirection();
        else
            AlignBeamToFacingDirection();
    }

    private void RotateBeam()
    {
        if (_motionInput == Vector2.zero)
        {
            // Smooth deceleration when no input is given
            _currentBeamAngularVelocity = Mathf.MoveTowards(_currentBeamAngularVelocity, 0, _beamAcceleration * Time.fixedDeltaTime);
            _beam.localEulerAngles += new Vector3(0, 0, _currentBeamAngularVelocity * Time.fixedDeltaTime);
            return;
        }

        // Calculate the target angle from input direction
        float _targetAngle = Mathf.Atan2(_motionInput.y, _motionInput.x) * Mathf.Rad2Deg;
        float _delta = Mathf.DeltaAngle(_beam.localEulerAngles.z, _targetAngle);
        float _maxDelta = _beamRotationSpeedDegreesPerSecond;

        // If the difference between the current angle and target angle is very small, snap to target angle
        if (Mathf.Abs(_delta) < 1f) // Threshold for snapping
        {
            _currentBeamAngularVelocity = 0;
            _beam.localEulerAngles = new Vector3(0, 0, _targetAngle);  // Snap to target angle
        }
        else
        {
            // Smooth rotation towards target angle
            _currentBeamAngularVelocity = Mathf.MoveTowards(_currentBeamAngularVelocity, Mathf.Sign(_delta) * _maxDelta, _beamAcceleration * Time.fixedDeltaTime);
            _beam.localEulerAngles += new Vector3(0, 0, _currentBeamAngularVelocity * Time.fixedDeltaTime);
        }
    }
    private void AlignBeamToMotionDirection() 
    {
        float angle = Mathf.Atan2(_motionInput.y, _motionInput.x) * Mathf.Rad2Deg; 
        _beam.localEulerAngles = new Vector3
        (
            _beam.localEulerAngles.x,
            _beam.localEulerAngles.y,
            angle
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

    private void OnUseItem()
    {
        if (PlayerMovementController.Instance.PlayerState.Value != PlayerMovementController.PlayerStates.Birding)
            return;
        StartCoroutine(EndGame());
    }

    private IEnumerator EndGame()
    {
        _logger.Info("Birding Game Ended");
        if (_sun != null) _sun.FadeInLight(0.3f);
        _currentBeamAngularVelocity = 0;
        yield return null;
        gameObject.SetActive(false);
        PlayerMovementController.Instance.PlayerState.Value = PlayerMovementController.PlayerStates.Idle;
    }
}
