using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public struct InCrossfade : IComponentData
{
     
    public float3 lastFramePosition;
    public quaternion lastFrameQuaternion;
    public int crossfadeFrame;

}
