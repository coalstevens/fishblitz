using UnityEngine;

// the roof is huge so gonna let infinite birds land on it.
// hopefully thats not an issue
public class AbandonedShed : MonoBehaviour, BirdBrain.IPerchableHighElevation
{
    [SerializeField] Collider2D _perch;
    Bounds _perchBounds;
    private SpriteRenderer _renderer;

    void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _perchBounds = _perch.bounds;
    }

    public Vector2 GetPositionTarget()
    {
        // Return random point in collider
        Vector2 randomPoint;
        do
        {
            float x = Random.Range(_perchBounds.min.x, _perchBounds.max.x);
            float y = Random.Range(_perchBounds.min.y, _perchBounds.max.y);
            randomPoint = new Vector2(x, y);
        } while (!_perch.OverlapPoint(randomPoint));

        return randomPoint;
    }

    public int GetSortingOrder()
    {
        return _renderer.sortingOrder;
    }

    public bool IsThereSpace()
    {
        return true;
    }

    public void OnBirdEntry(BirdBrain bird)
    {
        // do nothing
    }

    public void OnBirdExit(BirdBrain bird)
    {
        // do nothing
    }

    public void ReserveSpace(BirdBrain bird)
    {
        // do nothing
    }

}
