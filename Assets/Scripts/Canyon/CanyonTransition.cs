using OysterUtils;
using UnityEngine;

public class CanyonTransition : MonoBehaviour
{
    [SerializeField] private string _label;
    [SerializeField] private SoundData _sound;
    [SerializeField] private AudioSource _audioSource;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        CanyonManager mgr = Store.Get<CanyonManager>();
        if (mgr == null) return;

        CanyonExit exit = GetComponentInParent<CanyonExit>();
        if (exit != null)
        {
            PlaySound();
            mgr.TakeExit(exit.ExitId, _label, exit.TargetBiome);
            return;
        }

        CanyonEntrance entrance = GetComponentInParent<CanyonEntrance>();
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
