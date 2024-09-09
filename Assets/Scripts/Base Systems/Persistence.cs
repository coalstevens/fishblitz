using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace ColePersistence
{
    public static class JsonPersistence
    {
        private static bool IsWebGL()
        {
            return Application.platform == RuntimePlatform.WebGLPlayer;
        }

        private static string GetPersistencePath(string relativePath)
        {
            if (IsWebGL())
            {
                return relativePath;
            }
            return System.IO.Path.Combine(Application.persistentDataPath, relativePath);
        }

        public static Task PersistJson<T>(T item, string relativePath)
        {
            string json = JsonConvert.SerializeObject(item);

            if (IsWebGL())
            {
                PlayerPrefs.SetString(relativePath, json);
                PlayerPrefs.Save();
                return Task.CompletedTask;
            }
            else
            {
                return System.IO.File.WriteAllTextAsync(GetPersistencePath(relativePath), json);
            }
        }

        public async static Task<T> FromJson<T>(string relativePath)
        {
            if (IsWebGL())
            {
                if (PlayerPrefs.HasKey(relativePath))
                {
                    string json = PlayerPrefs.GetString(relativePath);
                    return JsonConvert.DeserializeObject<T>(json);
                }
                return default;
            }
            else
            {
                string json = await System.IO.File.ReadAllTextAsync(GetPersistencePath(relativePath));
                return JsonConvert.DeserializeObject<T>(json);
            }
        }

        public static bool JsonExists(string relativePath)
        {
            if (IsWebGL())
            {
                return PlayerPrefs.HasKey(relativePath);
            }
            else
            {
                return System.IO.File.Exists(GetPersistencePath(relativePath));
            }
        }
    }
}