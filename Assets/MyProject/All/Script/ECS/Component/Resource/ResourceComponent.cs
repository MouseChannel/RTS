using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using FixedMath;

 
public struct ResourceComponent : IComponentData
{
    
    public FixedVector2 position;
    public int resourceNo;
    public int resourcePositionIndex;
    public int stopNo;
    public int stopPositionIndex;

}
