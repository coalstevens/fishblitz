using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Collider2D))]
public class Maron : MonoBehaviour, InteractInput.IInteractable, ISaveableComponent
{
    private enum MaronStates
    {
        Idle,
        Greeted,
        Offered,
        Sawmill,
        Warped,
    }

    [SerializeField] private MaronStates _initialState = MaronStates.Idle;
    [SerializeField] private bool _isSawmillMaron;
    [SerializeField] private float _viewRange = 5f;
    [SerializeField] private float _minWaitBeforeWarpSecs = 1f;
    [SerializeField] private SceneNames _warpScene = SceneNames.CanyonStart;
    [SerializeField] private Vector3 _warpPosition = Vector3.zero;

    [System.Serializable]
    private class State
    {
        public int Current;
    }

    public string ComponentId => "Maron";

    private DialogueController _dialogueController;
    private MaronStates _state;
    private float _offerTime;

    private void Awake()
    {
        _state = _initialState;
        _dialogueController = GetComponentInChildren<DialogueController>();
        Assert.IsNotNull(_dialogueController, "Maron needs a DialogueController on a child object.");
    }

    private void ResolvePresence()
    {
        if (_isSawmillMaron)
        {
            bool present = _state == MaronStates.Warped;
            if (present)
            {
                _state = MaronStates.Sawmill;
                SceneSaveLoadManager.SavePlayerComponents();
            }
            gameObject.SetActive(present);
        }
        else if (_state != MaronStates.Idle)
        {
            _state = MaronStates.Idle;
        }
    }

    private void Update()
    {
        if (_state != MaronStates.Idle)
            return;

        if (Vector2.Distance(PlayerMovement.Instance.transform.position, transform.position) <= _viewRange)
        {
            _state = MaronStates.Greeted;
            _dialogueController.PostMessage("hey.");
        }
    }

    public bool CursorInteract(Vector3 cursorLocation)
    {
        switch (_state)
        {
            case MaronStates.Idle:
            case MaronStates.Greeted:
                _state = MaronStates.Offered;
                _offerTime = Time.time;
                _dialogueController.PostMessage("i can take you to the sawmill");
                return true;

            case MaronStates.Offered:
                if (Time.time - _offerTime < _minWaitBeforeWarpSecs)
                    return false;
                WarpPlayer();
                return true;

            case MaronStates.Sawmill:
                _dialogueController.PostMessage(Random.value < 0.5f ? "hey." : "hi.");
                return true;

            case MaronStates.Warped:
            default:
                return false;
        }
    }

    private void WarpPlayer()
    {
        _state = MaronStates.Warped;
        SceneSaveLoadManager.SavePlayerComponents();
        PlayerSceneData.PendingSpawnPosition = _warpPosition;
        PlayerSceneData.HasPendingSpawn = true;
        LevelChanger.ChangeLevel(_warpScene);
    }

    public string CaptureStateAsJson()
    {
        return JsonConvert.SerializeObject(new State { Current = (int)_state });
    }

    public void RestoreStateFromJson(string json)
    {
        var _stateData = JsonConvert.DeserializeObject<State>(json);
        if (_stateData == null)
            return;
        _state = (MaronStates)_stateData.Current;
        _offerTime = Time.time;
        ResolvePresence();
    }
}
