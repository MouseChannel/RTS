using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public struct PathFindParams : IComponentData
{
    public int startPosition;
    public int endPosition;
    
}
