using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BoxData", menuName = "Quest/BoxData")]
public class BoxData : ScriptableObject
{
    [Serializable]
    public class RequiredObject
    {
        public WeightyObjectType Type;
        public int Quantity;
    }

    public List<RequiredObject> RequiredObjects = new();
}
