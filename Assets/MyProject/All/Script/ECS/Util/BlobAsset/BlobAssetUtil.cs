using System.Collections.Concurrent;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using Hash128 = Unity.Entities.Hash128;



// public struct BlobExposedFramePositionData
// {
//     public BlobArray<SingleFrameData> singleFrameData;
// }
public class BlobAssetUtil
{
    public static Dictionary<int, Mesh> animationMesh = new Dictionary<int, Mesh>();
    public static BlobAssetStore animationBlobStore = new BlobAssetStore();
    public static ConcurrentDictionary<string, Mesh> animationMeshDic = new ConcurrentDictionary<string, Mesh>();

      

    // public static void AddAnimationElement(string stringName, AnimationScriptableObject anim)
    // {
    //     // if (animationBlobDic.ContainsKey(name)) return;
    //     var name = new Hash128(stringName);
    //     if (animationBlobStore.Contains<AnimationBlobElement>(name)) return;

    //     Profiler.BeginSample("BlobTest");
    //     BlobBuilder blobBuilder = new BlobBuilder(Allocator.Temp);


    //     ref AnimationBlobElement animElement = ref blobBuilder.ConstructRoot<AnimationBlobElement>();
    //     animElement.name = stringName;

    //     animElement.animInitTimeInfo = new float4(
    //         0,
    //         anim.totalFrames,
    //         0,
    //         anim.length
    //     );
    //     animElement.animInfo = new float4(
    //         0,
    //         anim.vertexCount,
    //         anim.textureSize.x,
    //         anim.textureSize.y
    //     );
    //     animElement.animScale = new float3(
    //         anim.animScalar
    //     );
    //     animElement.length = anim.length;
    //     animElement.totalFrames = anim.totalFrames;
    //     // var frameLength = anim.exposedFramePositionData.Length;
    //     // BlobBuilderArray<BlobExposedFramePositionData> exposedFramePositionData = blobBuilder.Allocate(ref animElement.exposedFramePositionData, frameLength);
    //     // var exposedTransformObjectLength = anim.exposedFramePositionData[0].singleFrameData.Length;

    //     // for (int i = 0; i < frameLength; i++)
    //     // {
    //     //     BlobBuilderArray<SingleFrameData> singleFramePositionData = blobBuilder.Allocate(ref exposedFramePositionData[i].singleFrameData, exposedTransformObjectLength);

    //     //     for (int j = 0; j < exposedTransformObjectLength; j++)
    //     //     {
    //     //         singleFramePositionData[j] = anim.exposedFramePositionData[i].singleFrameData[j];
    //     //     }

    //     // }
    //     // animElement.exposedFramePositionData = anim.exposedFramePositionData;

    //     BlobAssetReference<AnimationBlobElement> animationBlobElementRef
    //     = blobBuilder.CreateBlobAssetReference<AnimationBlobElement>(Allocator.Persistent);
    //     blobBuilder.Dispose();
    //     animationBlobStore.TryAdd(name, animationBlobElementRef);
    //     // animationBlobDic.Add(name, animationBlobElementRef);



    //     Profiler.EndSample();
    // }

    // // public static AnimationBlobElement GetAnimationBlob(string animationName){

    // }
}
