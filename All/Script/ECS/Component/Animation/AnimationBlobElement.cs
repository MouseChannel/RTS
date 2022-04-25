using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

using Unity.Collections;

public unsafe struct AnimationBlobElement
{
   



    public int totalFrames;
    public float length;
    public int index;
    public int meshIndex;
    public int2 textureSize;
    public int vertexCount;
    public int framesPerTexture;
    public float3 scale;
    // public int2 randerRange;




}

