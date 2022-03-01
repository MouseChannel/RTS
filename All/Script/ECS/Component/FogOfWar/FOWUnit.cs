using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using RVO;
using FixedMath;

public struct FOWUnit : IComponentData
{
    public FixedVector2 position;
    public int range;
}
