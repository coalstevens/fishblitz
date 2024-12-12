
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Journal : MonoBehaviour
{
    [Range(0f, 1f)][SerializeField] private float solidOpacity = 1.0f;
    [Range(0f, 1f)][SerializeField] private float fadedOpacity = 0.3f;
    [Header("Pages")]
    [SerializeField] private Transform _leftPage;
    [SerializeField] private Transform _cursor;
    [SerializeField] private Transform _rightPage;
    [Header("NoteBook")]
    [SerializeField] private TextMeshProUGUI _noteBookTitle;
    [SerializeField] private List<Image> _seasonIcons = new();
    [SerializeField] private List<Image> _dayPeriodIcons = new();
    [SerializeField] private Image _numerator;
    [SerializeField] private Image _denominator;
    [SerializeField] private List<Sprite> _numbersTo24RightJustified = new();
    [SerializeField] private List<Sprite> _numbersTo24LeftJustified = new();
    private Dictionary<string, PlayerData.BirdingLogEntry> _playerBirdLogLookup;
    private Transform[][] _pagesLookup;
    private (int i, int j) _pagePointer;
    private int _numRows = 4;
    private int _numColumns = 6;
    private int _totalEntries;

    void Start()
    {
        if (_seasonIcons.Count != 4)
            Debug.LogError("Season icons are assigned incorrectly in the journal.");
        if (_dayPeriodIcons.Count != 4)
            Debug.LogError("Day period icons are assigned incorrectly in the journal.");
        InitializePlayerBirdLogLookup();
        var _allEntries = CollectActiveEntries();
        _totalEntries = _allEntries.Count;
        InitializePagesLookupTable(_allEntries);
        RefreshPages(PlayerData.Instance.PlayerBirdingLog);
        UpdateNoteBook();
        StartCoroutine(UpdateCursorAfterDelay());
    }

    private IEnumerator UpdateCursorAfterDelay() {
        yield return null;
        UpdateCursorPosition();
    }

    private void UpdateCursorPosition() {
        _cursor.position = _pagesLookup[_pagePointer.i][_pagePointer.j].GetChild(1).transform.position;
    }

    private void InitializePlayerBirdLogLookup()
    {
        _playerBirdLogLookup = PlayerData.Instance.PlayerBirdingLog.CaughtBirds.ToDictionary(b => b.Name, b => b);
    }

    private List<Transform> CollectActiveEntries() {
        List<Transform> _allEntries = new();
        foreach (Transform child in _leftPage)
            if(child.gameObject.activeSelf)
                _allEntries.Add(child);
        foreach (Transform child in _rightPage)
            if(child.gameObject.activeSelf)
                _allEntries.Add(child);
        return _allEntries;
    }

    private void InitializePagesLookupTable(List<Transform> allEntries)
    {
        _pagePointer = (0,0);
        _pagesLookup = new Transform[_numRows][];
        for (int i = 0; i < _numRows; i++)
            _pagesLookup[i] = new Transform[_numColumns];
        
        if (_totalEntries > _numColumns * _numRows)
            Debug.LogError("Too many entries in journal.");

        int k = 0;
        // Map entries to left page
        for (int i = 0; i < _numRows; i++)
        {
            for (int j = 0; j < _numColumns / 2; j++)
            {
                if (k >= _totalEntries)
                    return;
                _pagesLookup[i][j] = allEntries[k];
                k++;
            }
        }
        // right page
        for (int i = 0; i < _numRows; i++)
        {
            for (int j = _numColumns / 2; j < _numColumns; j++)
            {
                if (k >= _totalEntries)
                    return;
                _pagesLookup[i][j] = allEntries[k];
                k++;
            }
        }
    }

    public void OnMoveCursor(InputValue value)
    {
        if (TryMoveJournalPointer(value.Get<Vector2>())) {
            UpdateCursorPosition();
            UpdateNoteBook();
        }
    }

    private void UpdateNoteBook()
    {
        // get the entry for the selected bird from the player log if it exists
        string _birdName = _pagesLookup[_pagePointer.i][_pagePointer.j].gameObject.name;
        RefreshNotebook(_playerBirdLogLookup.TryGetValue(_birdName, out var entry) ? entry : null);
    }

    private bool TryMoveJournalPointer(Vector2 inputDirection)
    {
        int newRow = _pagePointer.i + (int)inputDirection.y * -1;
        int newCol = _pagePointer.j + (int)inputDirection.x;

        if (newRow < 0 || newRow >= _numRows || newCol < 0 || newCol >= _numColumns)
            return false;

        if (_pagesLookup[newRow][newCol] == null)
            return false;

        _pagePointer.i = newRow;
        _pagePointer.j = newCol;
        return true;
    }

    private PlayerData.BirdingLogEntry GetBirdLogEntry(string birdname, PlayerData.BirdingLog birdingLog)
    {
        return _playerBirdLogLookup.TryGetValue(birdname, out var entry) ? entry : null;
    }

    private void RefreshPages(PlayerData.BirdingLog birdingLog)
    {
        _numerator.sprite = _numbersTo24RightJustified[_playerBirdLogLookup.Count];
        _denominator.sprite = _numbersTo24LeftJustified[_totalEntries];

        foreach (Transform _child in _leftPage)
        {
            bool _isBirdInLog = _playerBirdLogLookup.ContainsKey(_child.gameObject.name);
            _child.GetChild(0).gameObject.SetActive(!_isBirdInLog); // Set ? Icon
            _child.GetChild(1).gameObject.SetActive(_isBirdInLog); // Set bird Icon
        }

        foreach (Transform _child in _rightPage)
        {
            bool _isBirdInLog = _playerBirdLogLookup.ContainsKey(_child.gameObject.name);
            _child.GetChild(0).gameObject.SetActive(!_isBirdInLog); // Set ? Icon
            _child.GetChild(1).gameObject.SetActive(_isBirdInLog); // Set bird Icon
        }
    }

    private void RefreshNotebook(PlayerData.BirdingLogEntry selectedEntry)
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
