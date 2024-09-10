using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace ColePersistence
{
  public static class JsonPersistence
  {
    private static string GetPersistencePath(string relativePath)
    {
      return Path.Combine(Application.persistentDataPath, relativePath);
    }

    public static void PersistJson<T>(T item, string relativePath)
    {
      string json = JsonConvert.SerializeObject(item);
      File.WriteAllText(GetPersistencePath(relativePath), json);
    }

    public static T FromJson<T>(string relativePath)
    {
      string json = File.ReadAllText(GetPersistencePath(relativePath));
      return JsonConvert.DeserializeObject<T>(json);
    }

    public static bool JsonExists(string relativePath) {
      return File.Exists(GetPersistencePath(relativePath));
    }
  }
}