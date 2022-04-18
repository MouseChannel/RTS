using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

using Unity.Collections;

public struct AnimationBlobElement
{
    public FixedString512Bytes name;
    /// <summary>
    ///  need add new float4(0,0,_shaderTime.y,_shaderTime.y)
    /// </summary>
    public float4 animInitTimeInfo;
    /// <summary>
    /// need add new float4(textureStartIndex ,0 ,0 ,0)
    /// </summary>
    public float4 animInfo;
    public float3 animScale;

    // public ExposedFramePositionData[] exposedFramePositionData;
    /// <summary>
    /// 第一维是frame，第二维是exposedTransforms
    /// </summary>
    public BlobArray<BlobExposedFramePositionData> exposedFramePositionData;

    public int totalFrames;
    public float length;



}

