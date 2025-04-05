using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class ObjectPooling : MonoBehaviour
{
    [SerializeField] private static Logger _logger = new();
    [HideInInspector] public static List<PooledObjectInfo> Pools = new();
    private static Transform _container;

    public class PooledObjectInfo
    {
        public string LookupString;
        public List<GameObject> InactiveObjects = new();
    }

    private void Awake()
    {
        _container = transform;
    }

    public static GameObject SpawnObject(GameObject prefab, Vector2 position, Quaternion rotation)
    {
        PooledObjectInfo pool = Pools.Find(p => p.LookupString == prefab.name);

        if (pool == null)
        {
            pool = new PooledObjectInfo { LookupString = prefab.name };
            Pools.Add(pool);
        }

        GameObject spawnableObj = pool.InactiveObjects.FirstOrDefault();

        if (spawnableObj == null)
        {
            _logger.Info($"Instantiating new {prefab.name} in pool: ");
            spawnableObj = Instantiate(prefab, position, rotation, _container);
        }
        else
        {
            _logger.Info($"Reusing existing {prefab.name} in pool ");
            spawnableObj.transform.position = position;
            spawnableObj.transform.rotation = rotation;
            pool.InactiveObjects.Remove(spawnableObj);
            spawnableObj.SetActive(true);
        }

        Assert.IsNotNull(spawnableObj);
        return spawnableObj;
    }

    public static void ReturnObjectToPool(GameObject obj)
    {
        string _name = obj.name.Substring(0, obj.name.IndexOf("(Clone)"));
        PooledObjectInfo pool = Pools.Find(p => p.LookupString == _name);
        if (pool != null)
        {
            pool.InactiveObjects.Add(obj);
            obj.SetActive(false);
        }
        else
        {
            Debug.LogWarning($"Object {_name} not found in pool.");
        }
    }
}
