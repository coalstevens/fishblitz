using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TopRightHUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _clockText;
    [SerializeField] private TextMeshProUGUI _dateText;

    [SerializeField] private Sprite _springFrame;
    [SerializeField] private Sprite _summerFrame;
    [SerializeField] private Sprite _fallFrame;
    [SerializeField] private Sprite _winterFrame;
    private Inventory _inventory;
    private Image _frame;

    void Start()
    {
        _inventory = GameObject.FindWithTag("Inventory").GetComponent<Inventory>();
        _frame = GetComponent<Image>();
        
        GameClock.Instance.GameMinute.When((curr, prev) => prev % 5 == 0, (curr, prev) => UpdateClockText());
        GameClock.Instance.GameHour.OnChange((curr, prev) => UpdateClockText());
        GameClock.Instance.GameDay.OnChange((curr, prev) => UpdateDateText());
        GameClock.Instance.GameSeason.OnChange((curr, prev) => UpdateSeasonFrame());

        UpdateClockText();
        UpdateDateText();
        UpdateSeasonFrame();
    }

    void UpdateSeasonFrame()
    {
        _frame.sprite = GameClock.Instance.GameSeason.Value switch {
            GameClock.Seasons.Spring => _springFrame,
            GameClock.Seasons.EndOfSpring => _springFrame,
            GameClock.Seasons.Summer => _summerFrame,
            GameClock.Seasons.EndOfSummer => _summerFrame,
            GameClock.Seasons.Fall => _fallFrame,
            GameClock.Seasons.EndOfFall => _fallFrame,
            GameClock.Seasons.Winter => _winterFrame,
            GameClock.Seasons.EndOfWinter => _winterFrame,
            _ => null
        };
    }

    void UpdateClockText()
    {
        // 24h clock
        _clockText.text = GameClock.Instance.GameHour.Value.ToString() + ":";
        _clockText.text += GameClock.Instance.GameMinute.Value < 10 ? "0" : ""; // add a leading zero for <10 min
        _clockText.text += GameClock.Instance.GameMinute.Value.ToString();
    }

    void UpdateDateText()
    {
        // i guess i was indecisive about whether the side seasons have their own date numbers
        // hence this workaround
        int _gameDay = GameClock.Instance.GameDay.Value;
        // _gameDay += GameClock.Instance.GameSeason.Value switch {
        //     GameClock.Seasons.Spring => 0,
        //     GameClock.Seasons.Summer => 0,
        //     GameClock.Seasons.Fall => 0,
        //     GameClock.Seasons.Winter => 0,
        //     GameClock.Seasons.EndOfSpring => 10,
        //     GameClock.Seasons.EndOfSummer => 10,
        //     GameClock.Seasons.EndOfFall => 10,
        //     GameClock.Seasons.EndOfWinter => 10,  
        //     _ => 0,
        // };

        // "1st" thru "15th" 
        _dateText.text = $"The {_gameDay}";
        _dateText.text += _gameDay switch
        {
            1 => "st",
            2 => "nd",
            3 => "rd",
            _ => "th"
        };
    }

}
