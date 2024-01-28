using ReactiveUnity;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WoodStove : MonoBehaviour, IInteractable, IHeatSource, IWorldObject
{
    private Animator _animator;
    private Inventory _inventory;
    private GameClock _gameClock;
    private enum StoveStates {Dead, Ready, Hot, Embers};
    private Reactive<StoveStates> _stoveState = new Reactive<StoveStates>(StoveStates.Dead);
    private PulseLight _fireLight;
    private int _fireDurationCounterGameHours;

    [Header("Embers Settings")]
    [SerializeField] float _embersMinIntensity = 0.2f;
    [SerializeField] float _embersMaxIntensity = 1.0f;
    [SerializeField] private int _embersDurationGameHours = 1;

    [Header("Hot Fire Settings")]
    [SerializeField] float _fireMinIntensity = 1.3f;
    [SerializeField] float _fireMaxIntensity = 2f;
    [SerializeField] private int _hotFireDurationGameHours = 1;
    private Temperature _sourceTemperature;
    private const string IDENTIFIER = "WoodStove";
    public Temperature SourceTemperature {
        get => _sourceTemperature;
        set => _sourceTemperature = value;
    }

    public Collider2D InteractCollider { 
        get {
            Collider2D _collider = GetComponent<Collider2D>();
            if (_collider != null) {
                return _collider;
            }
            else {
                Debug.LogError("WoodStove does not have a collider component.");
                return null;
            }
        }
    }

    public string Identifier {
        get => IDENTIFIER;
    }

    public int State {
        get => (int)_stoveState.Value; 
        set => _stoveState.Value = (StoveStates) value; 
    }

    void Start()
    {
        _animator = GetComponent<Animator>();
        _inventory = GameObject.FindGameObjectWithTag("Inventory").GetComponent<Inventory>();
        _gameClock = GameObject.FindGameObjectWithTag("GameClock").GetComponent<GameClock>();
        _fireLight = transform.GetComponentInChildren<PulseLight>();
        _stoveState.OnChange((curr,prev) => OnStateChange());
        _gameClock.GameHour.OnChange((curr, prev) => OnGameHourChange());
        OnStateChange();
    }

    private void OnGameHourChange()
    { 
        switch (_stoveState.Value) 
        {
            case StoveStates.Hot:
                _fireDurationCounterGameHours++;
                if (_fireDurationCounterGameHours >= _hotFireDurationGameHours) {
                    _stoveState.Value = StoveStates.Embers;
                }
                break;
            case StoveStates.Embers:
                _fireDurationCounterGameHours++;
                if (_fireDurationCounterGameHours >= _hotFireDurationGameHours + _embersDurationGameHours) {
                    _stoveState.Value = StoveStates.Dead;
                }
                break;
        } 
    }

    void OnStateChange()
    {
        switch (_stoveState.Value) 
        {
            case StoveStates.Dead:
                EnterDead();
                break;
            case StoveStates.Ready:
                EnterReady();
                break;
            case StoveStates.Hot:
                EnterHot();
                break;
            case StoveStates.Embers:
                EnterEmbers();
                break;
        } 
        ShareHeatSourceTemperatureInParent();
    }

    private void EnterHot() {
        _fireDurationCounterGameHours = 0;
        _animator.speed = 1f;
        _animator.Play("HotFire");
        _sourceTemperature = Temperature.Hot;
        _fireLight.gameObject.SetActive(true);
        _fireLight.SetIntensity(_fireMinIntensity, _fireMaxIntensity);
    }

    private void EnterEmbers() {
        _animator.speed = 0.05f;
        _animator.Play("Embers");
        _sourceTemperature = Temperature.Warm;
        _fireLight.gameObject.SetActive(true);
        _fireLight.SetIntensity(_embersMinIntensity, _embersMaxIntensity);
    }

    private void EnterDead() {
        _animator.speed = 1f;
        _animator.Play("Dead");
        _sourceTemperature = Temperature.Cold;
        _fireLight.gameObject.SetActive(false);
    }

    private void EnterReady() {
        _animator.speed = 1f;
        _animator.Play("Ready");
        _sourceTemperature = Temperature.Cold;
        _fireLight.gameObject.SetActive(false);
    }

    public bool CursorInteract(Vector3 cursorLocation) {
        switch (_stoveState.Value) 
        {
            case StoveStates.Dead:
                // Add wood to ashes
                if (_inventory.IsPlayerHolding("Firewood")) {
                    StokeFlame();
                    _stoveState.Value = StoveStates.Ready;
                    return true;
                }
                return false;
            case StoveStates.Ready:
                // Start fire
                NarratorSpeechController.Instance.PostMessage("The room gets warm...");
                _stoveState.Value = StoveStates.Hot;
                return true;
            case StoveStates.Hot:
                // state internal transition, stoke fire
                if (_inventory.IsPlayerHolding("Firewood")) {
                    StokeFlame();
                    NarratorSpeechController.Instance.PostMessage("You stoke the fire...");
                    return true;
                }
                return false;   
            case StoveStates.Embers:
                // Stoke fire
                if (_inventory.IsPlayerHolding("Firewood")) {
                    StokeFlame();
                    _stoveState.Value = StoveStates.Hot;
                    NarratorSpeechController.Instance.PostMessage("You stoke the fire...");
                    return true;
                }   
                return false;
            default:
                Debug.LogError("WoodStove guard handler defaulted.");
                return false;
        } 
    }

    private void StokeFlame() {
        _inventory.RemoveItem("Firewood", 1);
        _fireDurationCounterGameHours = 0;
    }
    private void ShareHeatSourceTemperatureInParent() {
        IHeatSensitive[] _heatSensitiveObjects = transform.parent.GetComponentsInChildren<IHeatSensitive>();
        foreach (IHeatSensitive obj in _heatSensitiveObjects) {
            obj.LocalTemperature = _sourceTemperature;
        }
    } 
}
