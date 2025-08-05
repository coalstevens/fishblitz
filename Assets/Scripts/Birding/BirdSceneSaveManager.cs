using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using NUnit.Framework;

[CreateAssetMenu(fileName = "NewBirdSceneSaveManager", menuName = "Birding/BirdSceneSaveManager")]
public class BirdSceneSaveManager : ScriptableObject
{
    [SerializeField] private Logger _logger = new();
    private Dictionary<string, string> _savedBirdsJsonBySceneName = new();
    public int LastSpawnTime = 0;

    [System.Serializable]
    public class BirdSaveData
    {
        public float xSpawnPosition; // Split cause you can't serialize Unity Vector2
        public float ySpawnPosition;
        public bool IsTagged;
        public string SpeciesName;
    }

    public void SaveBirds(List<BirdBrain> birds)
    {
        var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        Assert.IsFalse(_savedBirdsJsonBySceneName.ContainsKey(currentScene),
            $"Birds for scene '{currentScene}' are already saved. Please clear existing data before saving.");

        List<BirdSaveData> birdInstances = new();
        foreach (var bird in birds)
        {
            Assert.IsNotNull(bird, "BirdBrain is null in the list of birds to save.");
            Assert.IsNotNull(bird.SpeciesData, "BirdBrain is missing SpeciesData reference.");
            Assert.IsNotNull(bird.InstanceData, "BirdBrain is missing InstanceData reference.");

            BirdSaveData instanceData = new BirdSaveData()
            {
                xSpawnPosition = bird.transform.position.x,
                ySpawnPosition = bird.transform.position.y,
                IsTagged = bird.InstanceData.IsTagged.Value,
                SpeciesName = bird.SpeciesData.name
            };

            birdInstances.Add(instanceData);
        }

        _logger.Info($"Saving {birdInstances.Count} birds for scene '{currentScene}'");
        _savedBirdsJsonBySceneName[currentScene] = JsonConvert.SerializeObject(birdInstances);
    }

    public List<BirdSaveData> LoadBirds()
    {
        var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (_savedBirdsJsonBySceneName.TryGetValue(currentScene, out var birdInstancesJson))
        {
            List<BirdSaveData> savedBirds = JsonConvert.DeserializeObject<List<BirdSaveData>>(birdInstancesJson);
            _logger.Info($"Loaded {savedBirds.Count} birds for scene '{currentScene}'");
            return savedBirds;
        }
        _logger.Info($"No saved birds found for scene '{currentScene}'. Returning empty list.");
        return new List<BirdSaveData>();
    }


    public void ClearSceneSaveData()
    {
        var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (!_savedBirdsJsonBySceneName.ContainsKey(currentScene))
        {
            _logger.Info($"No saved birds found for scene '{currentScene}'. Nothing to clear.");
            return;
        }
        _savedBirdsJsonBySceneName.Remove(currentScene);
    }
}
