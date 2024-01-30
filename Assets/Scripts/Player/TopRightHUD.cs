using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TopRightHUD : MonoBehaviour
{
    private Inventory _inventory;
    private GameClock _gameClock;
    private TextMeshProUGUI _textBox;
    // Start is called before the first frame update
    void Start()
    {
        _inventory = GameObject.FindWithTag("Inventory").GetComponent<Inventory>();
        _gameClock = GameObject.FindWithTag("GameClock").GetComponent<GameClock>();
        _textBox = GetComponent<TextMeshProUGUI>();

        UpdateText();
        _gameClock.GameMinute.OnChange((curr, prev) => UpdateText());
        _inventory._gold.OnChange((curr, prev) => UpdateText());
    }

    void UpdateText() {
        string _textString = "";
        _textString += _gameClock.GameHour.Value.ToString() + "h" + _gameClock.GameMinute.Value.ToString() + "m\n";
        _textString += _gameClock.SeasonNames[(int) _gameClock.GameSeason.Value] + " " + _gameClock.GameDay.Value.ToString() + "\n";
        _textString += "Year: " + _gameClock.GameYear.Value.ToString() + "\n";
        _textString += _inventory.Gold.ToString() + "G";

        _textBox.text = _textString;
    }
}
