using UnityEngine;

public class TheWayNorthTransition : MonoBehaviour
{
    [SerializeField] private string _label;
    [SerializeField] private SoundData _sound;
    [SerializeField] private AudioSource _audioSource;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.transform.root.CompareTag("Player")) return;

        TheWayNorth mgr = TheWayNorth.Instance;
        if (mgr == null) return;

        TheWayNorthExit exit = GetComponentInParent<TheWayNorthExit>();
        if (exit != null)
        {
            PlaySound();
            mgr.TakeExit(exit.ExitId, _label);
            return;
        }

        TheWayNorthEntrance entrance = GetComponentInParent<TheWayNorthEntrance>();
        if (entrance != null)
        {
            PlaySound();
            mgr.UseEntrance(entrance.EntranceId, _label);
        }
    }

    private void PlaySound()
    {
        if (_sound != null)
            AudioManager.PlaySFX(_audioSource, _sound);
    }
}
