using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BigRock : MonoBehaviour, IPerchable
{
    private List<Collider2D> _perches = new();
    private List<BirdBrain> _perchOccupier = new();
    private int _targetPerchIndex = new();

    void Start()
    {
        _perches = GetComponentsInChildren<Collider2D>().ToList();
        foreach (var _perch in _perches)
            _perchOccupier.Add(null);
    }

    public bool AreBirdsFrightened()
    {
        return false;
    }

    public Vector2 GetPositionTarget()
    {
        // This should only be run after IsThereSpace() is called
        // TODO: This is not ideal, should write a function that combines functionality

        Bounds bounds = _perches[_targetPerchIndex].bounds;
        Vector2 randomPoint;

        do
        {
            float x = Random.Range(bounds.min.x, bounds.max.x);
            float y = Random.Range(bounds.min.y, bounds.max.y);
            randomPoint = new Vector2(x, y);
        } while (!_perches[_targetPerchIndex].OverlapPoint(randomPoint));

        return randomPoint;
    }

    public bool IsThereSpace()
    {
        int _count = _perches.Count;
        int _startIndex = UnityEngine.Random.Range(0, _count);

        for (int i = 0; i < _count; i++)
        {
            int _currentIndex = (_startIndex + i) % _count;
            if (_perchOccupier[_currentIndex] == null)
            {
                _targetPerchIndex = _currentIndex;
                return true;
            }
        }
        return false;
    }

    public void OnBirdEntry(BirdBrain bird)
    {
        // do nothing
    }

    public void OnBirdExit(BirdBrain bird)
    {
        for (int i = 0; i < _perchOccupier.Count; i++)
        {
            if (_perchOccupier[i] == bird)
            {
                _perchOccupier[i] = null;
                return;
            }
        }
        Debug.LogError("Unexpected Code Path.");
    }

    public int GetSortingOrder()
    {
        return GetComponent<SpriteRenderer>().sortingOrder;
    }

    public void ReserveSpace(BirdBrain bird)
    {
        _perchOccupier[_targetPerchIndex] = bird;
    }
}
