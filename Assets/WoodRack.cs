using System.Collections;
using System.Collections.Generic;
using ReactiveUnity;
using UnityEngine;
using UnityEngine.UI;

public class WoodRack : MonoBehaviour
{
    private SpriteRenderer _spriteRender;
    private Reactive<int> _numWetLogs = new Reactive<int>(3);
    private Reactive<int> _numDryLogs = new Reactive<int>(0);
    private int _capacity = 18;
    [SerializeField] Sprite _empty;
    [SerializeField] Sprite _threeLogs;
    [SerializeField] Sprite _sixLogs;
    [SerializeField] Sprite _elevenLogs;
    [SerializeField] Sprite _fourteenLogs;
    [SerializeField] Sprite _eighteenLogs;

    void Start()
    {
        _spriteRender = GetComponent<SpriteRenderer>();
        _numWetLogs.OnChange((prev,curr) => UpdateRack());
        _numDryLogs.OnChange((prev,curr) => UpdateRack());
        UpdateRack();
    }

    void UpdateRack() {
        switch (_numDryLogs.Value + _numWetLogs.Value) {
            case 0:
                _spriteRender.sprite = _empty;
                break;
            case 3:
                _spriteRender.sprite = _threeLogs;
                break;
            case 6:
                _spriteRender.sprite = _sixLogs;
                break;
            case 11:
                _spriteRender.sprite = _elevenLogs;
                break;
            case 14:
                _spriteRender.sprite = _fourteenLogs;
                break;
            case 18:
                _spriteRender.sprite = _eighteenLogs;
                break;
            default:
                break;
        }
    }

    public bool AddWetLog() {
        if (_numWetLogs.Value + _numDryLogs.Value + 1 < _capacity) {
            _numWetLogs.Value++;
            return true;
        }
        else {
            return false;
        }
    }

    public bool AddDryLog() {
        if (_numWetLogs.Value + _numDryLogs.Value + 1 < _capacity) {
            _numDryLogs.Value++;
            return true;
        }
        else {
            return false;
        }
    }
    
    public bool RemoveDryLog() {
        if (_numDryLogs.Value >= 1) {
            //inventory add dry log
            return true;
        }
        else {
            return false;
        }
    }
}
