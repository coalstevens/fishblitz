using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class BirdingGame : MonoBehaviour
{
    [SerializeField] private float _beamRotationSpeed = 2f;
    private PlayerMovementController _playerMovementController;
    private Vector2 _motionInput = Vector2.zero;
    private Transform _beam;
    void Start()
    {
        _playerMovementController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovementController>();
        _beam = transform.GetChild(0);
    }

    private void FixedUpdate()
    {
        RotateBeam();
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
    }

    public void Stop()
    {
        gameObject.SetActive(false);
    }

    public void OnMove(InputValue value)
    {
        if (_playerMovementController.PlayerState.Value == PlayerStates.Birding)
            _motionInput = value.Get<Vector2>();
    }
}
