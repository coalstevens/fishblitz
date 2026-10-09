using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

public class CannonTrap : MonoBehaviour
{
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private CompassDirection _fireDirection = CompassDirection.South;
    [SerializeField] private float _fireInterval = 1f;
    [SerializeField] private Vector2 _projectileSpawnOffset = Vector2.zero;

    private Coroutine _firingLoop;

    private void OnEnable()
    {
        Assert.IsNotNull(_projectilePrefab);
        Assert.IsTrue(_fireInterval > 0);

        _firingLoop = StartCoroutine(FiringLoop());
    }

    private void OnDisable()
    {
        if (_firingLoop != null)
            StopCoroutine(_firingLoop);
    }

    private IEnumerator FiringLoop()
    {
        while (true)
        {
            Fire();
            yield return new WaitForSeconds(_fireInterval);
        }
    }

    private void Fire()
    {
        Vector2 direction = GetDirectionVector(_fireDirection);
        Vector2 spawnPos = (Vector2)transform.position + direction * 0.5f + _projectileSpawnOffset;

        GameObject projectileObj = ObjectPooling.SpawnObject(_projectilePrefab, spawnPos, Quaternion.identity);
        projectileObj.transform.localRotation = Quaternion.FromToRotation(Vector2.left, direction);

        Projectile projectile = projectileObj.GetComponent<Projectile>();
        projectile.Launch(direction);
    }

    private static Vector2 GetDirectionVector(CompassDirection direction)
    {
        return direction switch
        {
            CompassDirection.North => Vector2.up,
            CompassDirection.South => Vector2.down,
            CompassDirection.West => Vector2.left,
            CompassDirection.East => Vector2.right,
            CompassDirection.NorthEast => new Vector2(1f, 1f).normalized,
            CompassDirection.NorthWest => new Vector2(-1f, 1f).normalized,
            CompassDirection.SouthEast => new Vector2(1f, -1f).normalized,
            CompassDirection.SouthWest => new Vector2(-1f, -1f).normalized,
            _ => Vector2.down,
        };
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 direction = GetDirectionVector(_fireDirection);
        Vector2 origin = (Vector2)transform.position + _projectileSpawnOffset;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(origin, origin + direction * 2f);
    }
}