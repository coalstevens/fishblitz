using UnityEngine;

public class AudioPlayerManager : MonoBehaviour
{
    private static AudioPlayerManager _instance = null;
    private AudioSource _audio;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            return;
        }
        if (_instance == this) return;
        Destroy(gameObject);
    }

    void Start()
    {
        _audio = GetComponent<AudioSource>();
        _audio.Play();
    }
}