using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

[MaterialProperty("_AnimTimeInfo", MaterialPropertyFormat.Float4)]

public struct _AnimTimeInfo : IComponentData
{
    public float4 Value;
}

[MaterialProperty("_AnimInfo", MaterialPropertyFormat.Float4)]

public struct _AnimInfo : IComponentData
{
    public float4 Value;
}
[MaterialProperty("_AnimScalar", MaterialPropertyFormat.Float3)]
public struct _AnimScalar : IComponentData
{
    public float3 Value;
}
[MaterialProperty("_AnimTextureIndex", MaterialPropertyFormat.Float)]
public struct _AnimTextureIndex : IComponentData
{
    public float Value;
}


[MaterialProperty("_CrossfadeAnimInfo", MaterialPropertyFormat.Float4)]
public struct _CrossfadeAnimInfo : IComponentData
{
    public float4 Value;
}
[MaterialProperty("_CrossfadeAnimScalar", MaterialPropertyFormat.Float3)]
public struct _CrossfadeAnimScalar : IComponentData
{
    public float3 Value;
}

[MaterialProperty("_CrossfadeAnimTextureIndex", MaterialPropertyFormat.Float)]
public struct _CrossfadeAnimTextureIndex : IComponentData
{
    public float Value;
}

[MaterialProperty("_CrossfadeStartTime", MaterialPropertyFormat.Float)]
public struct _CrossfadeStartTime : IComponentData
{
    public float Value;
}

[MaterialProperty("_CrossfadeEndTime", MaterialPropertyFormat.Float)]
public struct _CrossfadeEndTime : IComponentData
{
    public float Value;
}


