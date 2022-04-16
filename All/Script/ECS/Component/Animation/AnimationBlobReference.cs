using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
public struct AnimationData : IComponentData
{
    public BlobAssetReference<AnimationBlobElement> currentAnimation;
    public int currentFrame;
    public float currentTime;
}

public struct ExposedAnimation : IComponentData{
    public BlobAssetReference<AnimationBlobElement> currentAnimation;
    public int exposedIndex;
}
