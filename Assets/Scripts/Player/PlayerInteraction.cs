using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerInteraction : MonoBehaviour
{
    private struct Candidate
    {
        public Collider2D Collider;
        public float Score;
        public float Distance;
    }

    [Header("Scoring")]
    [SerializeField] private float _angleWeight = 0.7f;
    [SerializeField] private float _distanceWeight = 0.3f;

    [Header("Region")]
    [SerializeField] private float _interactRadius = 1.75f;
    [SerializeField] private float _deadZoneRadius = 0.35f;
    [SerializeField] private float _maxAngleDeg = 60f;
    [SerializeField] private float _fallbackReach = 1f;
    [SerializeField] private LayerMask _targetingMask;

    [Header("Put Down")]
    [SerializeField] private int _putDownRange = 1;

    private static readonly Color RegionColor = new(1f, 0.92f, 0.23f, 0.85f);
    private static readonly Color DeadZoneColor = new(1f, 0.25f, 0.25f, 0.8f);
    private static readonly Color FallbackColor = new(0.3f, 0.9f, 1f, 0.9f);

    private Grid _grid;
    private PlayerMovement _playerMovement;
    private WorldObjectOccupancyMap _worldObjectOccupancyMap;
    private readonly List<Candidate> _candidates = new();
    private readonly HashSet<Object> _seen = new();
    private Collider2D _resolveCollider;
    private int _queryFrame = -1;

    public Vector3Int GridPosition => _grid.WorldToCell(ResolvePoint);

    public Vector3 ResolvePoint
    {
        get
        {
            RunCandidateQuery();
            if (_resolveCollider != null)
                return _resolveCollider.ClosestPoint(transform.position);
            return transform.position + (Vector3)(InteractDirection * _fallbackReach);
        }
    }

    public Vector2 InteractDirection
    {
        get
        {
            if (_playerMovement == null)
                _playerMovement = GetComponent<PlayerMovement>();
            Vector2 motion = _playerMovement.CurrentMotion;
            if (motion.sqrMagnitude > Mathf.Epsilon)
                return motion.normalized;
            return DirectionToVector(_playerMovement.Direction.Value);
        }
    }

    private void OnEnable()
    {
        _playerMovement = GetComponent<PlayerMovement>();
        _grid = FindFirstObjectByType<Grid>();
        Assert.IsNotNull(_grid, "Grid not found in scene.");

        _worldObjectOccupancyMap = FindFirstObjectByType<WorldObjectOccupancyMap>();
        Assert.IsNotNull(_worldObjectOccupancyMap, "WorldObjectOccupancyMap not found in scene.");

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _grid = FindFirstObjectByType<Grid>();
        _worldObjectOccupancyMap = FindFirstObjectByType<WorldObjectOccupancyMap>();
    }

    public T FindTarget<T>() where T : class
    {
        RunCandidateQuery();
        foreach (Candidate candidate in _candidates)
        {
            T target = candidate.Collider.GetComponent<T>();
            if (target == null && candidate.Collider.attachedRigidbody != null)
                target = candidate.Collider.attachedRigidbody.GetComponent<T>();
            if (target != null)
            {
                _resolveCollider = candidate.Collider;
                return target;
            }
        }
        return null;
    }

    public string FindInteractableTileMapByTags(List<string> tilemapTags)
    {
        List<string> foundInteractableTags = new();
        Tilemap[] tilemaps = FindObjectsByType<Tilemap>(FindObjectsSortMode.None);
        Vector3Int cursorPosition = GridPosition;

        foreach (Tilemap tilemap in tilemaps)
            if (IsWorldPositionInTilemap(tilemap, cursorPosition) && tilemapTags.Contains(tilemap.tag))
                foundInteractableTags.Add(tilemap.tag);

        if (foundInteractableTags.Count == 0)
            return null;

        if (foundInteractableTags.Count > 1)
            Debug.LogWarning($"There are {foundInteractableTags.Count} tilemaps with matching tags on this cursor location.");

        return foundInteractableTags[0];
    }

    public bool TryGetUnoccupiedTileNearPlayer(out Vector3Int tile)
    {
        Vector3Int origin = GridPosition;
        Vector2 aim = InteractDirection;
        tile = default;
        bool found = false;
        int nearestRing = int.MaxValue;
        float bestAlignment = float.MinValue;

        for (int x = -_putDownRange; x <= _putDownRange; x++)
        {
            for (int y = -_putDownRange; y <= _putDownRange; y++)
            {
                Vector3Int candidate = origin + new Vector3Int(x, y, 0);
                if (_worldObjectOccupancyMap.CheckOccupied(candidate))
                    continue;

                int ring = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                Vector2 offset = new(x, y);
                float alignment = offset.sqrMagnitude > Mathf.Epsilon
                    ? Vector2.Dot(offset.normalized, aim)
                    : 1f;

                if (!found || ring < nearestRing || (ring == nearestRing && alignment > bestAlignment))
                {
                    found = true;
                    nearestRing = ring;
                    bestAlignment = alignment;
                    tile = candidate;
                }
            }
        }

        if (found)
            return true;

        Debug.Log("No unoccupied tiles in put down range.");
        tile = default;
        return false;
    }

    private void RunCandidateQuery()
    {
        if (_queryFrame == Time.frameCount)
            return;

        _queryFrame = Time.frameCount;
        _resolveCollider = null;
        _candidates.Clear();
        _seen.Clear();

        Vector2 origin = transform.position;
        Vector2 aim = InteractDirection;

        foreach (Collider2D collider in Physics2D.OverlapCircleAll(origin, _interactRadius, _targetingMask))
        {
            Object key = collider.attachedRigidbody ? (Object)collider.attachedRigidbody : collider;
            if (!_seen.Add(key))
                continue;

            Vector2 offset = (Vector2)collider.ClosestPoint(origin) - origin;
            float distance = offset.magnitude;
            if (distance < _deadZoneRadius)
                continue;

            float angle = offset.sqrMagnitude > Mathf.Epsilon ? Vector2.Angle(aim, offset) : 0f;
            if (angle > _maxAngleDeg)
                continue;

            float angleScore = 1f - angle / _maxAngleDeg;
            float distanceScore = 1f - Mathf.Clamp01(distance / _interactRadius);
            float score = _angleWeight * angleScore + _distanceWeight * distanceScore;

            _candidates.Add(new Candidate { Collider = collider, Score = score, Distance = distance });
        }

        _candidates.Sort((a, b) =>
        {
            int byScore = b.Score.CompareTo(a.Score);
            return byScore != 0 ? byScore : a.Distance.CompareTo(b.Distance);
        });
    }

    private static bool IsWorldPositionInTilemap(Tilemap tilemap, Vector3 worldPosition)
    {
        Vector3Int cellPosition = tilemap.WorldToCell(worldPosition);
        return tilemap.GetTile(cellPosition) != null;
    }

    private static Vector2 DirectionToVector(CompassDirection direction)
    {
        switch (direction)
        {
            case CompassDirection.North:
                return Vector2.up;
            case CompassDirection.South:
                return Vector2.down;
            case CompassDirection.West:
                return Vector2.left;
            case CompassDirection.East:
                return Vector2.right;
            case CompassDirection.NorthEast:
                return new Vector2(0.70710678f, 0.70710678f);
            case CompassDirection.NorthWest:
                return new Vector2(-0.70710678f, 0.70710678f);
            case CompassDirection.SouthEast:
                return new Vector2(0.70710678f, -0.70710678f);
            case CompassDirection.SouthWest:
                return new Vector2(-0.70710678f, -0.70710678f);
            default:
                return Vector2.down;
        }
    }

    private void OnValidate()
    {
        _angleWeight = Mathf.Max(0f, _angleWeight);
        _distanceWeight = Mathf.Max(0f, _distanceWeight);
        _deadZoneRadius = Mathf.Max(0f, _deadZoneRadius);
        _interactRadius = Mathf.Max(_deadZoneRadius + 0.05f, _interactRadius);
        _maxAngleDeg = Mathf.Clamp(_maxAngleDeg, 1f, 180f);
        _fallbackReach = Mathf.Max(0f, _fallbackReach);
        _putDownRange = Mathf.Max(0, _putDownRange);
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 origin = transform.position;
        Vector2 aim = InteractDirection;
        float halfAngle = Mathf.Clamp(_maxAngleDeg, 1f, 180f);

        Vector2 edgeL = Rotate(aim, -halfAngle);
        Vector2 edgeR = Rotate(aim, halfAngle);

        Gizmos.color = RegionColor;
        Gizmos.DrawLine(origin, origin + edgeL * _interactRadius);
        Gizmos.DrawLine(origin, origin + edgeR * _interactRadius);
        DrawArc(origin, edgeL, halfAngle * 2f, _interactRadius);

        Gizmos.color = DeadZoneColor;
        Gizmos.DrawWireSphere(origin, _deadZoneRadius);

        Gizmos.color = FallbackColor;
        Vector2 fallback = origin + aim * _fallbackReach;
        Gizmos.DrawLine(origin, fallback);
        Gizmos.DrawWireCube(fallback, Vector3.one * 0.15f);
    }

    private static Vector2 Rotate(Vector2 vector, float degrees)
    {
        return Quaternion.Euler(0f, 0f, degrees) * vector;
    }

    private static void DrawArc(Vector2 origin, Vector2 from, float sweepDeg, float radius, int segments = 32)
    {
        Vector2 previous = origin + from * radius;
        for (int i = 1; i <= segments; i++)
        {
            Vector2 next = origin + Rotate(from, sweepDeg * i / segments) * radius;
            Gizmos.DrawLine(previous, next);
            previous = next;
        }
    }
}
