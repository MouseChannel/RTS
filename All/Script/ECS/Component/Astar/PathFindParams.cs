using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public struct PathFindParam : IComponentData
{
    public int endPosition;
    
}
public struct PathPosition : IBufferElementData
{
    public int2 position;
}
public struct CurrentPathIndex : IComponentData
{
   public int pathIndex;
}
