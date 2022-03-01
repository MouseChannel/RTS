using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public struct UnitTag : IComponentData
{
    public int id;
    public int faction;
    public int closestNeighbor;
}
