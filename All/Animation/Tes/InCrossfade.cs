using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public struct InCrossfade : IComponentData
{

     
    public float sumDeltaTime;
    public float3 beginScale;
    public float3 endScale;

}
