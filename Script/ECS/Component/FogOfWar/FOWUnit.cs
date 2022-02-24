using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public struct FOWUnit : IComponentData
{
    public int gridIndex;
    public int range;
}
