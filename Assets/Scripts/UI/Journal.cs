using System;
using System.Collections;
using System.Collections.Generic;
using ReactiveUnity;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameMenu : MonoBehaviour
{
    [Header("General")]
    [Range(0f, 1f)][SerializeField] private float solidOpacity = 1.0f;
    [Range(0f, 1f)][SerializeField] private float fadedOpacity = 0.3f;

    [Header("Menu Tabs")]
    [SerializeField] private Image _menuTabCursor;
    [SerializeField] private Sprite _playerTabHighlight;
    [SerializeField] private Sprite _journalTabHighlight;

    [Header("Journal Tabs")]
    [SerializeField] private Transform _journalTabCursor;
    [SerializeField] private float _fishTabCursorXPosition;
    [SerializeField] private float _birdsTabCursorXPosition;

    [Header("Pages")]
    [SerializeField] private Transform _entryCursor;
    [SerializeField] private Transform _birdsLeftPage;
    [SerializeField] private Transform _birdsRightPage;
    [SerializeField] private Transform _fishLeftPage;
    [SerializeField] private Transform _fishRightPage;

    [Header("NoteBook")]
    [SerializeField] private TextMeshProUGUI _noteBookTitle;
    [SerializeField] private List<Image> _seasonIcons = new();
    [SerializeField] private List<Image> _dayPeriodIcons = new();
    [SerializeField] private Image _numerator;
    [SerializeField] private Image _denominator;
    [SerializeField] private List<Sprite> _numbersTo24RightJustified = new();
    [SerializeField] private List<Sprite> _numbersTo24LeftJustified = new();
    enum Cursors { MENU_TABS, JOURNAL_TABS, ENTRIES }
    enum MenuTabs { PLAYER, JOURNAL };
    enum JournalTabs { BIRDS, FISH };
    private Reactive<Cursors> _activeCursor = new Reactive<Cursors>(Cursors.MENU_TABS);
    private Reactive<MenuTabs> _activeMenuTab = new Reactive<MenuTabs>(MenuTabs.JOURNAL);
    private Reactive<MenuTabs> _menuTabCursorState = new Reactive<MenuTabs>(MenuTabs.JOURNAL);
    private Reactive<JournalTabs> _activeJournalTab = new Reactive<JournalTabs>(JournalTabs.BIRDS);
    private Reactive<JournalTabs> _journalTabCursorState = new Reactive<JournalTabs>(JournalTabs.BIRDS);
    private Transform[][] _pagesLookup;
    private PlayerData.BirdingLog _playerBirdLog;
    private (int i, int j) _pagePointer;
    private int _rowsPerPage = 4;
    private int _columnsPerPage = 6;
    private int _numBirdEntries;
    private int _numFishEntries;

    private List<Action> _unsubscribeHooks = new();

    private void OnEnable()
    {
        if (_seasonIcons.Count != 4)
            Debug.LogError("Season icons are assigned incorrectly in the journal.");
        if (_dayPeriodIcons.Count != 4)
            Debug.LogError("Day period icons are assigned incorrectly in the journal.");

        _unsubscribeHooks.Add(_menuTabCursorState.OnChange(_ => RefreshMenuTabCursor()));
        _unsubscribeHooks.Add(_journalTabCursorState.OnChange(_ => RefreshJournalTabCursor()));
        _unsubscribeHooks.Add(_activeCursor.OnChange(curr => OnActiveCursorChange(curr)));

        _playerBirdLog = PlayerData.PlayerBirdingLog;
        var _birdsTabEntries = CollectTabEntries(_birdsLeftPage, _birdsRightPage);
        var _fishTabEntries = CollectTabEntries(_fishLeftPage, _fishRightPage);
        _numBirdEntries = _birdsTabEntries.Count;
        _numFishEntries = _fishTabEntries.Count;

        InitializeEntryCursor(_birdsTabEntries);
        RefreshPages();
        UpdateNoteBook();
        StartCoroutine(UpdateCursorAfterDelay());
    }

    private void OnDisable()
    {
        foreach (var _hook in _unsubscribeHooks)
            _hook();
        _unsubscribeHooks.Clear();
    }

    private IEnumerator UpdateCursorAfterDelay()
    {
        yield return null;
        MoveEntryCursor();
    }

    private void MoveEntryCursor()
    {
        _entryCursor.position = _pagesLookup[_pagePointer.i][_pagePointer.j].GetChild(1).transform.position;
    }

    private List<Transform> CollectTabEntries(Transform leftPage, Transform rightPage)
    {
        List<Transform> _allEntries = new();
        foreach (Transform child in leftPage)
            if (child.gameObject.activeSelf)
                _allEntries.Add(child);
        foreach (Transform child in rightPage)
            if (child.gameObject.activeSelf)
                _allEntries.Add(child);
        return _allEntries;
    }

    private void InitializeEntryCursor(List<Transform> tabEntries)
    {
        // Initialize 2D array to navigate
        _pagePointer = (0, 0);
        _pagesLookup = new Transform[_rowsPerPage][];
        for (int i = 0; i < _rowsPerPage; i++)
            _pagesLookup[i] = new Transform[_columnsPerPage];

        if (_numBirdEntries > _columnsPerPage * _rowsPerPage)
            Debug.LogError("Too many entries in journal.");

        // Map entries to left page
        int k = 0;
        for (int i = 0; i < _rowsPerPage; i++)
        {
            for (int j = 0; j < _columnsPerPage / 2; j++)
            {
                if (k >= _numBirdEntries)
                    return;
                _pagesLookup[i][j] = tabEntries[k];
                k++;
            }
        }

        // right page
        for (int i = 0; i < _rowsPerPage; i++)
        {
            for (int j = _columnsPerPage / 2; j < _columnsPerPage; j++)
            {
                if (k >= _numBirdEntries)
                    return;
                _pagesLookup[i][j] = tabEntries[k];
                k++;
            }
        }
    }

    // Player input method
    public void OnMoveCursor(InputValue value)
    {
        switch (_activeCursor.Value)
        {
            case Cursors.MENU_TABS:
                MoveMenuTabCursor(value.Get<Vector2>());
                break;
            case Cursors.JOURNAL_TABS:
                MoveJournalTabCursor(value.Get<Vector2>());
                break;
            case Cursors.ENTRIES:
                if (TryMoveEntryCursor(value.Get<Vector2>()))
                    UpdateNoteBook();
                break;
        }
    }

    private void UpdateNoteBook()
    {
        // get the entry for the selected bird from the player log if it exists
        string _birdName = _pagesLookup[_pagePointer.i][_pagePointer.j].gameObject.name;
        RefreshNotebook(_playerBirdLog.CaughtBirds.ContainsKey(_birdName) ? _birdName : null);
    }

    private void OnActiveCursorChange(Cursors newCursor)
    {
        _menuTabCursor.gameObject.SetActive(newCursor == Cursors.MENU_TABS);
        _journalTabCursor.gameObject.SetActive(newCursor == Cursors.JOURNAL_TABS);
        _entryCursor.gameObject.SetActive(newCursor == Cursors.ENTRIES);

        if (newCursor == Cursors.MENU_TABS)
            _menuTabCursorState.Value = _activeMenuTab.Value;
        else if (newCursor == Cursors.JOURNAL_TABS)
            _journalTabCursorState.Value = _activeJournalTab.Value;
    }

    private void OnActiveMenuTabChange(MenuTabs newTab)
    {
        return;
        // throw new NotImplementedException();
        // _activeMenuTab.Value = newTab;
        // RefreshMenuTabCursor();
    }

    private void RefreshMenuTabCursor()
    {
        if (_activeCursor.Value != Cursors.MENU_TABS)
            return;

        _menuTabCursor.sprite = _menuTabCursorState.Value switch
        {
            MenuTabs.PLAYER => _playerTabHighlight,
            MenuTabs.JOURNAL => _journalTabHighlight,
            _ => throw new ArgumentOutOfRangeException(nameof(_menuTabCursorState), "State not handled in UpdateMenuTabCursor.")
        };
    }

    private void RefreshJournalTabCursor()
    {
        if (_activeCursor.Value != Cursors.JOURNAL_TABS)
            return;

        if (_journalTabCursorState.Value == JournalTabs.BIRDS)
        {
            _journalTabCursor.localPosition = new Vector2
            (
                _birdsTabCursorXPosition,
                _journalTabCursor.localPosition.y
            );
        }
        else if (_journalTabCursorState.Value == JournalTabs.FISH)
        {
            _journalTabCursor.localPosition = new Vector2
            (
                _fishTabCursorXPosition,
                _journalTabCursor.localPosition.y
            );
        }
    }

    private void MoveJournalTabCursor(Vector2 inputDirection)
    {
        if ((int)inputDirection.x == 1 && Enum.IsDefined(typeof(JournalTabs), _journalTabCursorState.Value + 1))
            _journalTabCursorState.Value++;
        else if ((int)inputDirection.x == -1 && Enum.IsDefined(typeof(JournalTabs), _journalTabCursorState.Value - 1))
            _journalTabCursorState.Value--;
        else if ((int)inputDirection.y == -1)
            _activeCursor.Value = Cursors.ENTRIES;
        else if ((int)inputDirection.y == 1)
            _activeCursor.Value = Cursors.MENU_TABS;
    }

    private void MoveMenuTabCursor(Vector2 inputDirection)
    {
        if ((int)inputDirection.x == 1 && Enum.IsDefined(typeof(MenuTabs), _menuTabCursorState.Value + 1))
            _menuTabCursorState.Value++;
        else if ((int)inputDirection.x == -1 && Enum.IsDefined(typeof(MenuTabs), _menuTabCursorState.Value - 1))
            _menuTabCursorState.Value--;
        else if ((int)inputDirection.y == -1)
            _activeCursor.Value = Cursors.JOURNAL_TABS;
    }

    private bool TryMoveEntryCursor(Vector2 inputDirection)
    {
        int newRow = _pagePointer.i + (int)inputDirection.y * -1;
        int newCol = _pagePointer.j + (int)inputDirection.x;

        // moving off the top moves you to the tabs
        if (newRow < 0)
        {
            _activeCursor.Value = Cursors.JOURNAL_TABS;
            return false;
        }

        if (newRow >= _rowsPerPage || newCol < 0 || newCol >= _columnsPerPage)
            return false;

        if (_pagesLookup[newRow][newCol] == null)
            return false;

        _pagePointer.i = newRow;
        _pagePointer.j = newCol;
        MoveEntryCursor();
        return true;
    }

    private void RefreshPages()
    {
        _numerator.sprite = _numbersTo24RightJustified[_playerBirdLog.CaughtBirds.Count];
        _denominator.sprite = _numbersTo24LeftJustified[_numBirdEntries];

        foreach (Transform _child in _birdsLeftPage)
        {
            bool _isBirdInLog = _playerBirdLog.CaughtBirds.ContainsKey(_child.gameObject.name);
            _child.GetChild(0).gameObject.SetActive(!_isBirdInLog); // Set ? Icon
            _child.GetChild(1).gameObject.SetActive(_isBirdInLog); // Set bird Icon
        }

        foreach (Transform _child in _birdsRightPage)
        {
            bool _isBirdInLog = _playerBirdLog.CaughtBirds.ContainsKey(_child.gameObject.name);
            _child.GetChild(0).gameObject.SetActive(!_isBirdInLog); // Set ? Icon
            _child.GetChild(1).gameObject.SetActive(_isBirdInLog); // Set bird Icon
        }
    }

    private void RefreshNotebook(string birdName)
    {
        if (birdName == null)
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
        _noteBookTitle.text = birdName;
        PlayerData.BirdCapturePeriod _capturePeriod = _playerBirdLog.CaughtBirds[birdName];

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
