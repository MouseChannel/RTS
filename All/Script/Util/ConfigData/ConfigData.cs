using Unity.Entities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class StaticData
{
    public static readonly int gridWidth = 512;
    public static readonly int gridLength = gridWidth * gridWidth;
    public static  Vector4 _shaderTime { get { return Shader.GetGlobalVector("_Time"); } }
    public static float4 shaderTime;

}
