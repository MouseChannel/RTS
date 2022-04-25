using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
public struct AnimationData : IComponentData
{
    public BlobAssetReference<AnimationBlobElement> currentAnimation;
    public int currentFrame;
    public float currentTime;
    // public float length;
    // public int totalFrames;
    // public int currentAnimationIndex;
}

public struct CurrentAnimation{

}
