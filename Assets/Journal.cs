
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Journal : MonoBehaviour
{
    [System.Serializable]
    private class EntryIcons
    {
        public string Name;
        public GameObject BirdIcon;
        public GameObject QuestionMarkIcon;
    }

    [Range(0f, 1f)][SerializeField] private float solidOpacity = 1.0f;
    [Range(0f, 1f)][SerializeField] private float fadedOpacity = 0.3f;
    [SerializeField] private Transform _cursor;
    [SerializeField] private float _cursorRepeatIntervalSecs = 0.4f;
    [SerializeField] private List<Image> _seasonIcons = new();
    [SerializeField] private List<Image> _dayPeriodIcons = new();
    [SerializeField] private List<EntryIcons> _journalEntries = new();
    [SerializeField] private TextMeshProUGUI _noteBookTitle;
    private Dictionary<string, PlayerData.BirdingLogEntry> _playerBirdLogLookup;
    private float _lastCursorMoveTime = 0;
    private int _activeEntryIndex = 0;
    private int _numRows = 8;
    private int _numColumns = 3;


    void Start()
    {
        if (_seasonIcons.Count != 4)
            Debug.LogError("Season icons are assigned incorrectly in the journal.");
        if (_dayPeriodIcons.Count != 4)
            Debug.LogError("Day period icons are assigned incorrectly in the journal.");
        _playerBirdLogLookup = PlayerData.Instance.PlayerBirdingLog.CaughtBirds.ToDictionary(b => b.Name, b => b);

        UpdateJournal(PlayerData.Instance.PlayerBirdingLog);
        UpdateNoteBookInfo();        
    }

    public void OnMoveCursor(InputValue value)
    {
        // if (Time.time - _lastCursorMoveTime < _cursorRepeatIntervalSecs)
        //     return;

        int _cursorMove = GetCursorMove(value.Get<Vector2>()); 
        if (_cursorMove != 0)
        {
            // _lastCursorMoveTime = Time.time;
            _activeEntryIndex += _cursorMove;
            UpdateNoteBookInfo();
        }
    }

    private void UpdateNoteBookInfo() {
            _cursor.position = _journalEntries[_activeEntryIndex].BirdIcon.transform.position;
            // get the entry for the selected bird from the player log if it exists
            UpdateNoteBookInfo(_playerBirdLogLookup.TryGetValue(_journalEntries[_activeEntryIndex].Name, out var entry) ? entry : null);
    }

    private int GetCursorMove(Vector2 inputDirection) {
        // Mapping cursor movement of a 1D list of entries to a numRows x numColumns matrix
        // move up
        if (inputDirection.y == 1 && _activeEntryIndex - _numColumns >= 0)
            return -_numColumns;
        // move down
        else if (inputDirection.y == -1 && _activeEntryIndex + _numColumns < _journalEntries.Count)
            return _numColumns;
        else if (inputDirection.x == 1 && _activeEntryIndex + 1 < _journalEntries.Count)
        {
            // move right on right page. Can't if in last column
            if (_activeEntryIndex >= _numColumns * _numRows && (_activeEntryIndex + 1) % _numColumns != 0)
                return 1;
            // move right on left page. Jump to right page if in last column
            else
            {
                int _jumpDistance = _numColumns * _numRows - (_numColumns - 1);
                if ((_activeEntryIndex + 1) % 3 == 0 && _activeEntryIndex + _jumpDistance < _journalEntries.Count)
                    return _jumpDistance;
                else
                    return 1;
            }
        }
        else if (inputDirection.x == -1 && _activeEntryIndex - 1 >= 0)
        {
            // move left on right page. Jump to left page if in first column
            if (_activeEntryIndex >= _numColumns * _numRows)
            {
                int _jumpDistance = _numColumns * _numRows - (_numColumns - 1);
                if (_activeEntryIndex % 3 == 0 && _activeEntryIndex - _jumpDistance >= 0)
                    return  -_jumpDistance;
                else if (_activeEntryIndex - 1 >= 0)
                    return -1;
            }
            // move left on left page. Can't if in first column.
            else if (_activeEntryIndex - 1 >= 0 && _activeEntryIndex % 3 != 0)
                return -1;
        }
        return 0;
    }

    private PlayerData.BirdingLogEntry GetEntryOfBird(string birdname, PlayerData.BirdingLog birdingLog)
    {
        return _playerBirdLogLookup.TryGetValue(birdname, out var entry) ? entry : null;
    }

    private void UpdateJournal(PlayerData.BirdingLog birdingLog)
    {
        foreach (var _bird in birdingLog.CaughtBirds)
        {
            foreach (var _entry in _journalEntries)
            {
                bool _birdCaught = _entry.Name == _bird.Name;
                _entry.BirdIcon.SetActive(_birdCaught);
                _entry.QuestionMarkIcon.SetActive(!_birdCaught);
            }
        }
    }

    private void UpdateNoteBookInfo(PlayerData.BirdingLogEntry selectedEntry)
    {
        if (selectedEntry == null)
        {
            _noteBookTitle.text = "???";
            // Dim all icons
            for (int i = 0; i < 4; i++)
            {
                SetIconOpacity(_seasonIcons[i], true);
                SetIconOpacity(_dayPeriodIcons[i], true);
            }
            return;
        }
        _noteBookTitle.text = selectedEntry.Name;

        SetIconOpacity(_seasonIcons[0], selectedEntry.CaughtSeasons.Contains(GameClock.Seasons.Spring));
        SetIconOpacity(_seasonIcons[1], selectedEntry.CaughtSeasons.Contains(GameClock.Seasons.Summer));
        SetIconOpacity(_seasonIcons[2], selectedEntry.CaughtSeasons.Contains(GameClock.Seasons.Fall));
        SetIconOpacity(_seasonIcons[3], selectedEntry.CaughtSeasons.Contains(GameClock.Seasons.Winter));

        SetIconOpacity(_dayPeriodIcons[0], selectedEntry.CaughtDayPeriods.Contains(GameClock.DayPeriods.SUNRISE));
        SetIconOpacity(_dayPeriodIcons[1], selectedEntry.CaughtDayPeriods.Contains(GameClock.DayPeriods.DAY));
        SetIconOpacity(_dayPeriodIcons[2], selectedEntry.CaughtDayPeriods.Contains(GameClock.DayPeriods.SUNSET));
        SetIconOpacity(_dayPeriodIcons[3], selectedEntry.CaughtDayPeriods.Contains(GameClock.DayPeriods.NIGHT));
    }

    private void SetIconOpacity(Image icon, bool isDim)
    {
        icon.color = new Color
        (
            icon.color.r,
            icon.color.g,
            icon.color.b,
            isDim ? fadedOpacity : solidOpacity
        );
    }
}
