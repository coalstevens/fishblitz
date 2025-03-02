using UnityEngine;

[CreateAssetMenu(fileName = "Gerard", menuName = "Scriptable Objects/Gerard")]
public class Gerard : ScriptableObject
{
    public enum States { UnconsciousBeach, UnconsciousShed, AwakeShed }
    [SerializeField] private States _state = States.UnconsciousBeach;
    public States State => _state;
    public bool ReadyForNextState = false;

    void OnEnable()
    {
        SleepMenu.PlayerSlept += OnPlayerSleep;
    }
    void OnDisable()
    {
        SleepMenu.PlayerSlept -= OnPlayerSleep;
    }

    void OnPlayerSleep()
    {
        if (_state == States.UnconsciousBeach && ReadyForNextState)
        {
            _state = States.UnconsciousShed;
            ReadyForNextState = false;
            return;
        }

        if (_state == States.UnconsciousShed && ReadyForNextState)
        {
            _state = States.AwakeShed;
            ReadyForNextState = false;
        }
    }

}
