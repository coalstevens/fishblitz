using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using NUnit.Framework;

public class RenderBirdsInFront : MonoBehaviour
{
    private LayerMask birdLayer;
    private SpriteRenderer _myRenderer;
    [SerializeField] private Logger _logger = new();

    void Awake()
    {
        _myRenderer = GetComponent<SpriteRenderer>();
        birdLayer = 1 << LayerMask.NameToLayer("Birds");
        Assert.IsNotNull(_myRenderer);
        Assert.IsTrue(birdLayer != -1, $"The bird layer was not found {gameObject.name}");
        Assert.IsTrue(birdLayer != 0, $"There are no layers assigned to this script in: {gameObject.name}");
    }

    void LateUpdate()
    {
        Bounds myBounds = _myRenderer.bounds;
        Collider2D[] hits = Physics2D.OverlapBoxAll(myBounds.center, myBounds.size, 0f, birdLayer);

        HashSet<SortingGroup> processedBirds = new HashSet<SortingGroup>();

        foreach (var hit in hits)
        {
            _logger.Info($"{gameObject.name} hit bird: {hit.gameObject.name}");
            SortingGroup birdGroup = FindParentSortingGroup(hit.transform);
            if (birdGroup == null)
            {
                _logger.Info("No sorting group found");
                continue;
            }

            BirdBrain bird = birdGroup.GetComponent<BirdBrain>();
            if (bird.BirdState is BirdBrain.GroundedState)
            {
                _logger.Info("Bird is grounded");
                continue; // ignore if bird is grounded
            }

            if (processedBirds.Contains(birdGroup))
            {
                _logger.Info("Bird is already processed");
                continue; // Already processed this bird
            }

            SpriteRenderer birdRenderer = birdGroup.GetComponentInChildren<SpriteRenderer>();
            if (birdRenderer == null)
            {
                _logger.Info("No bird renderer found");
                continue;
            }

            if (birdRenderer.bounds.Intersects(myBounds))
            {
                _logger.Info($"Applying sorting order: {_myRenderer.sortingOrder+1}");
                birdGroup.sortingOrder = _myRenderer.sortingOrder + 1;
                _logger.Info($"Bird sorting order is {birdGroup.sortingOrder}");
                processedBirds.Add(birdGroup);
            }
        }
    }

    private SortingGroup FindParentSortingGroup(Transform t)
    {
        while (t != null)
        {
            SortingGroup sg = t.GetComponent<SortingGroup>();
            if (sg != null)
                return sg;
            t = t.parent;
        }
        return null;
    }
}