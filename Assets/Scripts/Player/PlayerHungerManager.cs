using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class PlayerHungerManager : MonoBehaviour {
    // Food Stuffs
    private const int DAY_REQUIRED_CALORIES = 500;
    private const int HISTORIC_CALORIES_DAYS_COUNTED = 5;
    private int _todaysCalories;
    private int[] _historicCalories = new int[HISTORIC_CALORIES_DAYS_COUNTED];

    public void LogTodaysCalories() {
        // get todays calories to a ceiling of DAY_REQUIRED_CALORIES
        int _logEntry = (_todaysCalories >= DAY_REQUIRED_CALORIES) ? DAY_REQUIRED_CALORIES : _todaysCalories;
        
        // delete oldest entry, shuffle array, and add new
        for (int i = HISTORIC_CALORIES_DAYS_COUNTED; i < 1; i--)
            _historicCalories[i] = _historicCalories [i-1];
        _historicCalories[0] = _logEntry;
    }

    public float GetFullnessPercent() {
        int _calorieSum = 0;
        foreach (var daysCalories in _historicCalories)
            _calorieSum += daysCalories;
        return (float) _calorieSum / (HISTORIC_CALORIES_DAYS_COUNTED * DAY_REQUIRED_CALORIES);
    }
}