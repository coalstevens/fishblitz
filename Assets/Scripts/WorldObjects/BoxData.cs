using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BoxData", menuName = "Quest/BoxData")]
public class BoxData : ScriptableObject
{
    public interface IBoxPrize
    {
        void AwardPrize();
    }

    [Header("Requirements")]
    public List<RequiredObject> RequiredObjects = new();

    [Header("Prize")]
    public GameObject PrizePrefab;
    public Vector3 PrizeSpawnOffset;

    [Serializable]
    public class RequiredObject
    {
        public WeightyObjectType Type;
        public int Quantity;
    }
}
