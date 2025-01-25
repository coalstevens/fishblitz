using System;
using System.Collections.Generic;
using ReactiveUnity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Journal : MonoBehaviour, GameMenuManager.IGameMenuPage
{
    private enum JournalCursors { TABS, ENTRIES };

    [Serializable]
    private class JournalPage
    {
        public Sprite FrameSprite;
        public CaptureLog Log;
        public Transform LeftPage;
        public Transform RightPage;
        public float PageTabCursorXPosition;
        [NonSerialized] public int NumberOfEntries;
        [NonSerialized] public List<Transform> Entries;
    }

    [Header("General")]
    [SerializeField] private Image _frame;
    [SerializeField] private List<JournalPage> _journalPages = new();
    [SerializeField] private Logger _logger = new();

    [Header("Cursors")]
    [SerializeField] private Transform _journalTabCursor;
    [SerializeField] private Transform _journalEntryCursor;

    [Header("Notebook")]
    [SerializeField] private TextMeshProUGUI _noteBookTitle;
    [SerializeField] private List<Image> _seasonIcons = new();
    [SerializeField] private List<Image> _dayPeriodIcons = new();
    [SerializeField] private Image _numerator;
    [SerializeField] private Image _denominator;
    [SerializeField] private List<Sprite> _numbersTo24RightJustified = new();
    [SerializeField] private List<Sprite> _numbersTo24LeftJustified = new();
    [Range(0f, 1f)][SerializeField] private float _solidOpacity = 1.0f;
    [Range(0f, 1f)][SerializeField] private float _fadedOpacity = 0.3f;

    private Reactive<JournalCursors> _activeJournalCursor = new Reactive<JournalCursors>(JournalCursors.TABS);
    private Reactive<int> _journalTabCursorIndex = new Reactive<int>(0);
    private Reactive<int> _currentJournalPageIndex = new Reactive<int>(0);
    private Transform[][] _entryLookupTable;
    private (int i, int j) _entryPointer;
    private const int _ROWS_PER_PAGE = 4;
    private const int _COLUMNS_PER_PAGE = 6;
    private List<Action> _unsubscribeHooks = new();

    public void LoadPage()
    {
        gameObject.SetActive(true);
        DisableCursor();

        foreach (var _journalPage in _journalPages)
        {
            _journalPage.Entries = CollectJournalPageEntries(_journalPage);
            _journalPage.NumberOfEntries = _journalPage.Entries.Count;
        }

        _unsubscribeHooks.Add(_journalTabCursorIndex.OnChange(_ => UpdateJournalTabCursorPosition()));
        _unsubscribeHooks.Add(_currentJournalPageIndex.OnChange((prev, curr) => LoadJournalPage(_journalPages[prev], _journalPages[curr])));
        _unsubscribeHooks.Add(_activeJournalCursor.OnChange(curr => OnActiveCursorChange(curr)));

        LoadJournalPage(null, _journalPages[_currentJournalPageIndex.Value]);
        _logger.Info($"Journal loaded, page set to {_currentJournalPageIndex.Value}");
    }

    public void UnloadPage()
    {
        _logger.Info("Journal unloaded");
        foreach (var hook in _unsubscribeHooks)
            hook();
        _unsubscribeHooks.Clear();
        gameObject.SetActive(false);
    }

    public void EnableCursor()
    {
        _logger.Info("Journal cursor enabled");
        _journalTabCursor.gameObject.SetActive(true);
        _journalEntryCursor.gameObject.SetActive(false);
        _journalTabCursorIndex.Value = _currentJournalPageIndex.Value;
    }

    public void DisableCursor()
    {
        _logger.Info("Journal cursor disabled");
        _journalTabCursor.gameObject.SetActive(false);
        _journalEntryCursor.gameObject.SetActive(false);
    }

    private void OnActiveCursorChange(JournalCursors curr)
    {
        if (curr == JournalCursors.TABS)
            ActivateTabCursor();
        else if (curr == JournalCursors.ENTRIES)
            ActivateEntryCursor();
        else
            Debug.LogError("Unexpected code path");
    }

    private void ActivateTabCursor()
    {
        _logger.Info("Switching to tab cursor");
        _journalTabCursorIndex.Value = _currentJournalPageIndex.Value;
        _journalEntryCursor.gameObject.SetActive(false);
        _journalTabCursor.gameObject.SetActive(true);
    }

    private void ActivateEntryCursor()
    {
        _logger.Info("Switching to entry cursor");
        InitializeEntryCursor(_journalPages[_currentJournalPageIndex.Value]);
        MoveEntryCursor();
        _journalTabCursor.gameObject.SetActive(false);
        _journalEntryCursor.gameObject.SetActive(true);
    }

    private void LoadJournalPage(JournalPage previousPage, JournalPage currentPage)
    {
        previousPage?.LeftPage.gameObject.SetActive(false);
        previousPage?.RightPage.gameObject.SetActive(false);

        currentPage.LeftPage.gameObject.SetActive(true);
        currentPage.RightPage.gameObject.SetActive(true);

        _frame.sprite = currentPage.FrameSprite;
        _numerator.sprite = _numbersTo24RightJustified[currentPage.Log.CaughtCreatures.Count];
        _denominator.sprite = _numbersTo24LeftJustified[currentPage.NumberOfEntries];

        foreach (Transform _child in currentPage.LeftPage)
        {
            bool _isNameInLog = currentPage.Log.CaughtCreatures.ContainsKey(_child.gameObject.name);
            _child.GetChild(0).gameObject.SetActive(!_isNameInLog); // Set ? Icon
            _child.GetChild(1).gameObject.SetActive(_isNameInLog); // Set bird Icon
        }

        foreach (Transform _child in currentPage.RightPage)
        {
            bool isNameInLog = currentPage.Log.CaughtCreatures.ContainsKey(_child.gameObject.name);
            _child.GetChild(0).gameObject.SetActive(!isNameInLog); // Set ? Icon
            _child.GetChild(1).gameObject.SetActive(isNameInLog); // Set bird Icon
        }

        InitializeEntryCursor(_journalPages[_currentJournalPageIndex.Value]);
        MoveEntryCursor();
        UpdateNotebookDisplay(_journalPages[_currentJournalPageIndex.Value]);
        _logger.Info($"Journal page loaded: {_currentJournalPageIndex.Value}");
    }

    public void Select()
    {
        switch (_activeJournalCursor.Value)
        {
            case JournalCursors.TABS:
                _logger.Info("Journal tab selected");
                if (_journalTabCursorIndex.Value != _currentJournalPageIndex.Value)
                    _currentJournalPageIndex.Value = _journalTabCursorIndex.Value;
                break;
            case JournalCursors.ENTRIES:
                _logger.Info("Journal entry selected");
                // do nothing
                break;
        }
    }

    public bool MoveCursor(Vector2 inputDirection)
    {
        if (_activeJournalCursor.Value == JournalCursors.TABS)
        {
            return MoveJournalTabCursor(inputDirection);
        }
        else if (_activeJournalCursor.Value == JournalCursors.ENTRIES)
        {
            if (TryMoveEntryCursor(inputDirection))
                UpdateNotebookDisplay(_journalPages[_currentJournalPageIndex.Value]);
        }
        return true;
    }

    private bool MoveJournalTabCursor(Vector2 inputDirection)
    {
        _logger.Info("Attempting to move journal tab cursor");
        // Returning false indicates that the cursor should switch to the menu tabs
        if ((int)inputDirection.y == 1)
            return false;

        if ((int)inputDirection.x == 1 && _journalTabCursorIndex.Value + 1 < _journalPages.Count)
        {
            _journalTabCursorIndex.Value++;
            _logger.Info($"Tab cursor moved to {_journalTabCursorIndex.Value}");
        }
        else if ((int)inputDirection.x == -1 && _journalTabCursorIndex.Value - 1 >= 0)
        {
            _journalTabCursorIndex.Value--;
            _logger.Info($"Tab cursor moved to {_journalTabCursorIndex.Value}");
        }
        else if ((int)inputDirection.y == -1)
            _activeJournalCursor.Value = JournalCursors.ENTRIES;
        return true;
    }

    private bool TryMoveEntryCursor(Vector2 inputDirection)
    {
        _logger.Info("Attempting to move journal entry cursor");
        int newRow = _entryPointer.i + (int)inputDirection.y * -1;
        int newCol = _entryPointer.j + (int)inputDirection.x;

        // If moving up from the top row, switch to tabs
        if (newRow < 0)
        {
            _activeJournalCursor.Value = JournalCursors.TABS;
            return false;
        }

        // If moving right from the right column, switch page if possible
        if (newCol >= _COLUMNS_PER_PAGE || (inputDirection.x > 0 && _entryLookupTable[newRow][newCol] == null))
        {
            _logger.Info("Moving right off the page");
            if (_currentJournalPageIndex.Value + 1 < _journalPages.Count)
                _currentJournalPageIndex.Value++;
            return false;
        }

        // If moving left from the left column, switch page if possible
        if (newCol < 0)
        {
            _logger.Info("Moving left off the page");
            if (_currentJournalPageIndex.Value - 1 >= 0)
                _currentJournalPageIndex.Value--;
            return false;
        }

        if (newRow >= _ROWS_PER_PAGE || _entryLookupTable[newRow][newCol] == null)
            return false;

        _entryPointer = (newRow, newCol);
        MoveEntryCursor();
        return true;
    }

    private void MoveEntryCursor() 
    {
        _journalEntryCursor.position = _entryLookupTable[_entryPointer.i][_entryPointer.j].GetChild(1).position;
    }


    private void InitializeEntryCursor(JournalPage page)
    {
        _logger.Info($"Entry cursor initializing for page {_currentJournalPageIndex.Value}");

        // Initialize 2D array to navigate
        _entryPointer = (0, 0);
        _entryLookupTable = new Transform[_ROWS_PER_PAGE][];
        for (int i = 0; i < _ROWS_PER_PAGE; i++)
            _entryLookupTable[i] = new Transform[_COLUMNS_PER_PAGE];

        if (page.NumberOfEntries > _COLUMNS_PER_PAGE * _ROWS_PER_PAGE)
            Debug.LogError("Too many entries in journal.");

        // Map entries to left page
        int k = 0;
        for (int i = 0; i < _ROWS_PER_PAGE; i++)
        {
            for (int j = 0; j < _COLUMNS_PER_PAGE / 2; j++)
            {
                if (k >= page.NumberOfEntries)
                    return;
                _entryLookupTable[i][j] = page.Entries[k];
                k++;
            }
        }

        // right page
        for (int i = 0; i < _ROWS_PER_PAGE; i++)
        {
            for (int j = _COLUMNS_PER_PAGE / 2; j < _COLUMNS_PER_PAGE; j++)
            {
                if (k >= page.NumberOfEntries)
                    return;
                _entryLookupTable[i][j] = page.Entries[k];
                k++;
            }
        }
    }

    private List<Transform> CollectJournalPageEntries(JournalPage page)
    {
        List<Transform> _allEntries = new();
        foreach (Transform child in page.LeftPage)
            if (child.gameObject.activeSelf)
                _allEntries.Add(child);
        foreach (Transform child in page.RightPage)
            if (child.gameObject.activeSelf)
                _allEntries.Add(child);
        return _allEntries;
    }

    private void UpdateJournalTabCursorPosition()
    {
        _logger.Info("Updating journal tab cursor position");
        if (_activeJournalCursor.Value != JournalCursors.TABS)
            return;

        _journalTabCursor.localPosition = new Vector2
        (
            _journalPages[_journalTabCursorIndex.Value].PageTabCursorXPosition,
            _journalTabCursor.localPosition.y
        );
    }

    private void UpdateNotebookDisplay(JournalPage page)
    {
        string _title = _entryLookupTable[_entryPointer.i][_entryPointer.j].gameObject.name;
        _title = page.Log.CaughtCreatures.ContainsKey(_title) ? _title : null;

        if (_title == null)
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

        _noteBookTitle.text = _title;
        CaptureLog.CapturePeriod _capturePeriod = page.Log.CaughtCreatures[_title];

        SetIconOpacity(_seasonIcons[0], !_capturePeriod.CaughtSeasons.Contains(GameClock.Seasons.Spring));
        SetIconOpacity(_seasonIcons[1], !_capturePeriod.CaughtSeasons.Contains(GameClock.Seasons.Summer));
        SetIconOpacity(_seasonIcons[2], !_capturePeriod.CaughtSeasons.Contains(GameClock.Seasons.Fall));
        SetIconOpacity(_seasonIcons[3], !_capturePeriod.CaughtSeasons.Contains(GameClock.Seasons.Winter));

        SetIconOpacity(_dayPeriodIcons[0], !_capturePeriod.CaughtDayPeriods.Contains(GameClock.DayPeriods.SUNRISE));
        SetIconOpacity(_dayPeriodIcons[1], !_capturePeriod.CaughtDayPeriods.Contains(GameClock.DayPeriods.DAY));
        SetIconOpacity(_dayPeriodIcons[2], !_capturePeriod.CaughtDayPeriods.Contains(GameClock.DayPeriods.SUNSET));
        SetIconOpacity(_dayPeriodIcons[3], !_capturePeriod.CaughtDayPeriods.Contains(GameClock.DayPeriods.NIGHT));
    }

    private void SetIconOpacity(Image icon, bool isDim)
    {
        icon.color = new Color
        (
            icon.color.r,
            icon.color.g,
            icon.color.b,
            isDim ? _fadedOpacity : _solidOpacity
        );
    }
}
