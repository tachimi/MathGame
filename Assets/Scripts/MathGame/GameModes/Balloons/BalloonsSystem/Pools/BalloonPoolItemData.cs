using System;
using BalloonsSystem;
using UnityEngine;

namespace Pools
{
    [Serializable]
    public class BalloonPoolItemData
    {
        [field: SerializeField] public Balloon Prefab { get; set; }
        [field: SerializeField] public int Amount { get; set; }
    }
}