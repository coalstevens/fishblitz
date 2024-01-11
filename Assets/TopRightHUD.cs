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
        _gameClock._gameMinute.OnChange((curr, prev) => UpdateText());
        _inventory._gold.OnChange((curr, prev) => UpdateText());
    }

    void UpdateText() {
        string _textString = "";
        _textString += _gameClock._gameHour.Value.ToString() + "h" + _gameClock._gameMinute.Value.ToString() + "m\n";
        _textString += _gameClock.SeasonNames[(int) _gameClock._gameSeason.Value] + " " + _gameClock._gameDay.Value.ToString() + "\n";
        _textString += "Year: " + _gameClock._gameYear.Value.ToString() + "\n";
        _textString += _inventory.Gold.ToString() + "G";

        _textBox.text = _textString;
    }
}
