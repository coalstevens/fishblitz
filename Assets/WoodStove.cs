using System.Collections;
using System.Collections.Generic;
using ReactiveUnity;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WoodStove : MonoBehaviour, ICursorInteractableObject
{
    // Start is called before the first frame update
    private Animator _animator;
    private enum StoveStates {Dead, Ready, Hot, Embers};
    private Reactive<StoveStates> _stoveState = new Reactive<StoveStates>(StoveStates.Dead);
    private PulseLight _fireLight;
    [Header("Embers Brightness")]
    [SerializeField] float _embersMinIntensity = 0.2f;
    [SerializeField] float _embersMaxIntensity = 1.0f;

    [Header("Fire Brightness")]
    [SerializeField] float _fireMinIntensity = 1.3f;
    [SerializeField] float _fireMaxIntensity = 2f;

    void Start()
    {
        _animator = GetComponent<Animator>();
        _fireLight = transform.GetComponentInChildren<PulseLight>();
        _stoveState.OnChange((curr,prev) => OnStateChange());
        OnStateChange();
    }
    void OnStateChange()
    {
        switch (_stoveState.Value) 
        {
            case StoveStates.Dead:
                _animator.speed = 1f;
                _animator.Play("Dead");
                transform.GetChild(0).gameObject.SetActive(false);
                break;
            case StoveStates.Ready:
                _animator.speed = 1f;
                _animator.Play("Ready");
                transform.GetChild(0).gameObject.SetActive(false);
                break;
            case StoveStates.Hot:
                _animator.speed = 1f;
                _animator.Play("HotFire");
                transform.GetChild(0).gameObject.SetActive(true);
                _fireLight.SetIntensity(_fireMinIntensity,_fireMaxIntensity);
                break;
            case StoveStates.Embers:
                _animator.speed = 0.05f;
                _animator.Play("Embers");
                transform.GetChild(0).gameObject.SetActive(true);
                _fireLight.SetIntensity(_embersMinIntensity,_embersMaxIntensity);
                break;
        } 
    }

    public void CursorAction(TileData tileData, Vector3 cursorLocation)
    {
        if((int) _stoveState.Value > 3) {
            _stoveState.Value = 0;
        }
        _stoveState.Value++;
    }
}
