using UnityEngine;

public class FishBarTrigger : MonoBehaviour
{
    [SerializeField] private Sprite _fulfilledSprite;
    [SerializeField] private Sprite _unfulfilledSprite;
    private bool _fulfilled = false;
    public bool Fulfilled {
        get => _fulfilled;
        set {
            _fulfilled = value;
            _spriteRenderer.sprite = _fulfilled ? _fulfilledSprite : _unfulfilledSprite;
            if(_fulfilled) 
                _audioSource.Play();
        }
    }
    
    private AudioSource _audioSource;
    private SpriteRenderer _spriteRenderer;
    private Collider2D _collider;
    private bool _oscillating = false;
    private float _oscillationLowerBound; 
    private float _oscillationUpperBound;
    private bool _movingUp;
    FishingRound _round;

    public void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _collider = GetComponent<Collider2D>();
        _audioSource = GetComponent<AudioSource>();
        Fulfilled = false;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        gameObject.SendMessageUpwards("PassedFishBarTrigger", this, SendMessageOptions.DontRequireReceiver);
    }

    public Collider2D GetCollider()
    {
        return _collider;
    }
    private void FixedUpdate() {
        if (_oscillating)
            Oscillate();
    }

    // Moves trigger transform up and down within oscillation bounds
    private void Oscillate() {
        float increment = _round.OscillatingSpeed * Time.fixedDeltaTime;
        if (_movingUp)
        {   
            if (transform.localPosition.y + increment > _oscillationUpperBound) {
                _movingUp = false;
                transform.Translate(0, -increment, 0);
                return;
            }
            transform.Translate(0, increment, 0);
        }
        else
        {
            if (transform.localPosition.y - increment < _oscillationLowerBound)
            {
                _movingUp = true;
                transform.Translate(0, increment, 0);
                return;
            }
            transform.Translate(0, -increment, 0);
        }
    }

    public void InitalizeOscillation(FishingRound fishingRound) {
        _round = fishingRound;
        _oscillating = true;
        ConfigureOscillationBounds();
        ConfigureRandomOscillationStart();
    }

    private void ConfigureOscillationBounds() {
        float _oscillationLength = _round.OscillationLengthNormalized * FishBar.TRIGGER_MAPPING_SLOPE;

        // trigger can not oscillate outside of game play area bounds
        float _playerAreaUpperBound = FishBar.TRIGGER_MAPPING_SLOPE + FishBar.TRIGGER_MAPPING_INTERCEPT;
        float _playAreaLowerBound = FishBar.TRIGGER_MAPPING_INTERCEPT;

        // by default the trigger spawn position is the center of oscillation range
        float _localUpperBound = transform.localPosition.y + _oscillationLength / 2;
        float _localLowerBound = transform.localPosition.y - _oscillationLength / 2;
        
        if (_playerAreaUpperBound < _localUpperBound) {
            _oscillationUpperBound = _playerAreaUpperBound;
            _oscillationLowerBound = _playerAreaUpperBound - _oscillationLength;
        }
        else if (_playAreaLowerBound > _localLowerBound) {
            _oscillationUpperBound = _playAreaLowerBound + _oscillationLength;
            _oscillationLowerBound = _playAreaLowerBound;
        }
        else {
            _oscillationUpperBound = _localUpperBound;
            _oscillationLowerBound = _localLowerBound;
        }
    }

    private void ConfigureRandomOscillationStart() {
        transform.localPosition = new Vector3 (0, Random.Range(_oscillationLowerBound, _oscillationUpperBound), 0);
        _movingUp = Random.Range(0,1) > 0.5;
    }
}
