using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace ColePersistence
{
  public static class JsonPersistence
  {
    private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
    {
      Converters = new System.Collections.Generic.List<JsonConverter> { new Vector3Converter() }
    };

    private static string GetPersistencePath(string relativePath)
    {
      return Path.Combine(Application.persistentDataPath, relativePath);
    }

    public static void PersistJson<T>(T item, string relativePath)
    {
      string json = JsonConvert.SerializeObject(item, SerializerSettings);
      File.WriteAllText(GetPersistencePath(relativePath), json);
    }

    public static T FromJson<T>(string relativePath)
    {
      string json = File.ReadAllText(GetPersistencePath(relativePath));
      return JsonConvert.DeserializeObject<T>(json, SerializerSettings);
    }

    public static bool JsonExists(string relativePath) {
      return File.Exists(GetPersistencePath(relativePath));
    }

    public static void DeleteFile(string relativePath) {
      string fullPath = GetPersistencePath(relativePath);
      if (File.Exists(fullPath))
        File.Delete(fullPath);
    }
  }
}