using TMPro;
using UnityEngine;

public class PlayerDialogueController : MonoBehaviour
{
    private static PlayerDialogueController _instance;
    public static PlayerDialogueController Instance
    {
        get
        {
            // If the instance doesn't exist, try to find it in the scene
            if (_instance == null)
            {
                _instance = FindObjectOfType<PlayerDialogueController>();

                // If it still doesn't exist, create a new GameObject with the singleton script attached
                if (_instance == null)
                {
                    Debug.Log("PlayerDialogueController does not exists.");
                }
            }

            return _instance;
        }
    }
    private TextMeshProUGUI _textBox;
    private Transform _player;
    private float _postedTime;
    public Vector3 offset = new Vector3(0, 3, 0); // Adjust as needed
    [SerializeField] private float _messageDurationSecs = 5f;
    [SerializeField] private float _fadeRateAlphaPerFrame = 0.005f;
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject); // Ensures that only one instance of the singleton exists
        }
    }
    void Start()
    {
        _textBox = GetComponentInChildren<TextMeshProUGUI>();
        _player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        // PlayerDialogueController.Instance.PostMessage("Good morning...");
        if (_textBox.text == "") {
            return;
        }

        _textBox.transform.position = _player.position + offset;

        // hold message
        if (Time.time - _postedTime < _messageDurationSecs) {
            return;
        }

        // fade message
        if (_textBox.alpha > _fadeRateAlphaPerFrame) {
            _textBox.alpha -=  _fadeRateAlphaPerFrame;  
        }
        else {
            _textBox.alpha = 0f;
            _textBox.text = "";
        }
    }

    public void PostMessage(string message) {
        _textBox.alpha = 1f;
        _textBox.text = message;
        _postedTime = Time.time;
    }
}

