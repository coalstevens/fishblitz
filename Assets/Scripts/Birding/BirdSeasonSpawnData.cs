using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "BirdSpawnCalendar", menuName = "Birding/BirdSpawnCalendar")]
public class BirdSeasonSpawnData : ScriptableObject
{
    [Tooltip("Each season contains exactly 3 weeks of spawn data.")]
    [SerializeField] private WeekData[] _weekData = new WeekData[3];

    [Serializable]
    public class SpeciesSpawnData
    {
        public BirdSpeciesData SpeciesData;
        public int SpawnCount;
        public List<Tilemap> SpawnAreas = new();
    }

    [Serializable]
    public class WeekData
    {
        [Tooltip("Birds that spawn this week.")]
        public List<SpeciesSpawnData> DailySpawns = new();
    }

    private void OnEnable()
    {
        // Ensure WeekData array and sublists are initialized
        if (_weekData == null || _weekData.Length != 3)
            _weekData = new WeekData[3];

        for (int i = 0; i < _weekData.Length; i++)
        {
            if (_weekData[i] == null)
                _weekData[i] = new WeekData();
            else if (_weekData[i].DailySpawns == null)
                _weekData[i].DailySpawns = new();
        }
    }

    public WeekData GetWeekData(int weekIndex)
    {
        if (weekIndex < 0 || weekIndex >= 3)
        {
            Debug.LogError("Invalid week index: " + weekIndex);
            return null;
        }

        return _weekData[weekIndex];
    }
}