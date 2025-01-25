using System;
using System.Collections.Generic;
using ReactiveUnity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Journal : MonoBehaviour, GameMenuManager.IGameMenuPage
{
    public enum JournalTabs { BIRDS, FISH };
    private enum Cursors { JOURNAL_TABS, ENTRIES };

    [Serializable]
    private class JournalPage
    {
        public Sprite frameSprite;
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

    [Header("Cursors")]
    [SerializeField] private Transform _journalTabCursor;
    [SerializeField] private Transform _entryCursor;

    [Header("Notebook")]
    [SerializeField] private TextMeshProUGUI _noteBookTitle;
    [SerializeField] private List<Image> _seasonIcons = new();
    [SerializeField] private List<Image> _dayPeriodIcons = new();
    [SerializeField] private Image _numerator;
    [SerializeField] private Image _denominator;
    [SerializeField] private List<Sprite> _numbersTo24RightJustified = new();
    [SerializeField] private List<Sprite> _numbersTo24LeftJustified = new();
    [Range(0f, 1f)][SerializeField] private float solidOpacity = 1.0f;
    [Range(0f, 1f)][SerializeField] private float fadedOpacity = 0.3f;

    private Reactive<Cursors> _activeCursor = new Reactive<Cursors>(Cursors.JOURNAL_TABS);
    private Reactive<int> _tabCursorTab = new Reactive<int>(0);
    private Reactive<int> _currentJournalPage = new Reactive<int>(0);
    private List<Action> _unsubscribeHooks = new();
    private Transform[][] _entryLookupTable;
    private (int i, int j) _entryPointer;
    private int _rowsPerPage = 4;
    private int _columnsPerPage = 6;
    [SerializeField] private Logger _logger = new();

    public void LoadPage()
    {
        gameObject.SetActive(true);
        DisableCursor();

        if (_seasonIcons.Count != 4)
            Debug.LogError("Season icons are assigned incorrectly in the journal.");
        if (_dayPeriodIcons.Count != 4)
            Debug.LogError("Day period icons are assigned incorrectly in the journal.");

        foreach (var _journalPage in _journalPages)
        {
            _journalPage.Entries = CollectJournalPageEntries(_journalPage);
            _journalPage.NumberOfEntries = _journalPage.Entries.Count;
        }

        _unsubscribeHooks.Add(_tabCursorTab.OnChange(_ => UpdateJournalTabCursorPosition()));
        _unsubscribeHooks.Add(_currentJournalPage.OnChange((prev, curr) => LoadJournalPage(_journalPages[prev], _journalPages[curr])));
        _unsubscribeHooks.Add(_activeCursor.OnChange(curr => OnActiveCursorChange(curr)));

        LoadJournalPage(null, _journalPages[_currentJournalPage.Value]);
        _logger.Info($"Journal loaded, page set to {_currentJournalPage.Value}");
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
        _entryCursor.gameObject.SetActive(false);
        _tabCursorTab.Value = 0;
    }

    public void DisableCursor()
    {
        _logger.Info("Journal cursor disabled");
        _journalTabCursor.gameObject.SetActive(false);
        _entryCursor.gameObject.SetActive(false);
    }

    private void OnActiveCursorChange(Cursors curr)
    {
        switch (curr)
        {
            case Cursors.JOURNAL_TABS:
                _logger.Info("Journal cursor set to tabs");
                _entryCursor.gameObject.SetActive(false);
                _journalTabCursor.gameObject.SetActive(true);
                _tabCursorTab.Value = _currentJournalPage.Value;
                break;
            case Cursors.ENTRIES:
                _logger.Info("Journal cursor set to entries");
                _journalTabCursor.gameObject.SetActive(false);
                _entryCursor.gameObject.SetActive(true);
                InitializeEntryCursor(_journalPages[_currentJournalPage.Value]);
                break;
        }
    }
    private void LoadJournalPage(JournalPage previousPage, JournalPage currentPage)
    {
        if (previousPage != null)
        {
            previousPage.LeftPage.gameObject.SetActive(false);
            previousPage.RightPage.gameObject.SetActive(false);
        }

        currentPage.LeftPage.gameObject.SetActive(true);
        currentPage.RightPage.gameObject.SetActive(true);
        
        _frame.sprite = currentPage.frameSprite;
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
        InitializeEntryCursor(_journalPages[_currentJournalPage.Value]);
        UpdateNotebookDisplay(_journalPages[_currentJournalPage.Value]);
        _logger.Info($"Journal page loaded: {_currentJournalPage.Value}");
    }

    public void Select()
    {
        switch (_activeCursor.Value)
        {
            case Cursors.JOURNAL_TABS:
                _logger.Info("Journal tab selected");
                if (_tabCursorTab.Value != _currentJournalPage.Value)
                    _currentJournalPage.Value = _tabCursorTab.Value;
                break;
            case Cursors.ENTRIES:
                _logger.Info("Journal entry selected");
                // do nothing
                break;
        }
    }

    public bool MoveCursor(Vector2 inputDirection)
    {
        if (_activeCursor.Value == Cursors.JOURNAL_TABS)
        {
            return MoveJournalTabCursor(inputDirection);
        }
        else if (_activeCursor.Value == Cursors.ENTRIES)
        {
            if (TryMoveEntryCursor(inputDirection))
                UpdateNotebookDisplay(_journalPages[_currentJournalPage.Value]);
        }
        return true;
    }

    private bool MoveJournalTabCursor(Vector2 inputDirection)
    {
        _logger.Info("Attempting to move journal tab cursor");
        // Returning false indicates that the cursor should switch to the menu tabs
        if ((int)inputDirection.y == 1)
            return false;

        if ((int)inputDirection.x == 1 && _tabCursorTab.Value + 1 < _journalPages.Count)
        {
            _tabCursorTab.Value++;
            _logger.Info($"Tab cursor moved to {_tabCursorTab.Value}");
        }
        else if ((int)inputDirection.x == -1 && _tabCursorTab.Value - 1 >= 0)
        {
            _tabCursorTab.Value--;
            _logger.Info($"Tab cursor moved to {_tabCursorTab.Value}");
        }
        else if ((int)inputDirection.y == -1)
            _activeCursor.Value = Cursors.ENTRIES;
        return true;
    }

    private bool TryMoveEntryCursor(Vector2 inputDirection)
    {
        _logger.Info("Attempting to move journal entry cursor");
        int newRow = _entryPointer.i + (int)inputDirection.y * -1;
        int newCol = _entryPointer.j + (int)inputDirection.x;

        // moving off the top moves you to the tabs
        if (newRow < 0)
        {
            _activeCursor.Value = Cursors.JOURNAL_TABS;
            return false;
        }

        if (newRow >= _rowsPerPage || newCol < 0 || newCol >= _columnsPerPage)
            return false;

        if (_entryLookupTable[newRow][newCol] == null)
            return false;

        _entryPointer.i = newRow;
        _entryPointer.j = newCol;
        _entryCursor.position = _entryLookupTable[_entryPointer.i][_entryPointer.j].GetChild(1).transform.position;
        return true;
    }

    private void InitializeEntryCursor(JournalPage page)
    {
        _logger.Info($"Entry cursor initializing for page {_currentJournalPage.Value}");

        // Initialize 2D array to navigate
        _entryPointer = (0, 0);
        _entryLookupTable = new Transform[_rowsPerPage][];
        for (int i = 0; i < _rowsPerPage; i++)
            _entryLookupTable[i] = new Transform[_columnsPerPage];

        if (page.NumberOfEntries > _columnsPerPage * _rowsPerPage)
            Debug.LogError("Too many entries in journal.");

        // Map entries to left page
        int k = 0;
        for (int i = 0; i < _rowsPerPage; i++)
        {
            for (int j = 0; j < _columnsPerPage / 2; j++)
            {
                if (k >= page.NumberOfEntries)
                    return;
                _entryLookupTable[i][j] = page.Entries[k];
                k++;
            }
        }

        // right page
        for (int i = 0; i < _rowsPerPage; i++)
        {
            for (int j = _columnsPerPage / 2; j < _columnsPerPage; j++)
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
        if (_activeCursor.Value != Cursors.JOURNAL_TABS)
            return;

        _journalTabCursor.localPosition = new Vector2
        (
            _journalPages[_tabCursorTab.Value].PageTabCursorXPosition,
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
            isDim ? fadedOpacity : solidOpacity
        );
    }
}
