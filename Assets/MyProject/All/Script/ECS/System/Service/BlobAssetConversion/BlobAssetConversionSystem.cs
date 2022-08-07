// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using Unity.Entities;
// using Unity.Collections;
// using FSG.MeshAnimator.ShaderAnimated;
// using Unity.Mathematics;

// public partial class BlobAssetConversionSystem : SystemBase
// {
//     private static Vector4 _shaderTime { get { return Shader.GetGlobalVector("_Time"); } }
//     public static HashSet<ShaderMeshAnimation> animationSet = new HashSet<ShaderMeshAnimation>();
//     public static NativeHashMap<FixedString512Bytes, BlobAssetReference<AnimationBlobElement>> animationBlobDic =
//             new NativeHashMap<FixedString512Bytes, BlobAssetReference<AnimationBlobElement>>(100, Allocator.Persistent);
//     protected override void OnDestroy()
//     {
//         animationBlobDic.Dispose();
//     }
//     protected override void OnStartRunning()
//     {
//         Debug.Log(animationSet.Count);

//         foreach (var i in animationSet)
//         {
//             using (BlobBuilder blobBuilder = new BlobBuilder(Allocator.Temp))
//             {
//                 ref AnimationBlobElement animElement = ref blobBuilder.ConstructRoot<AnimationBlobElement>();
//                 animElement.name = i.AnimationName;
//                 //need Add
//                 animElement.animInitTimeInfo = new float4(
//                     0,
//                     i.TotalFrames,
//                     0,
//                     i.length
//                 );
//                 animElement.animInfo = new float4(
//                     0,
//                     i.vertexCount,
//                     i.textureSize.x,
//                     i.textureSize.y
//                 );
//                 animElement.animScale = new float3(
//                     i.animScalar
//                 );

//                 BlobAssetReference<AnimationBlobElement> animationBlobElementRef =
//                 blobBuilder.CreateBlobAssetReference<AnimationBlobElement>(Allocator.Persistent);
//                 animationBlobDic.Add(i.AnimationName, animationBlobElementRef);

//             }
//         }
 




//     }
//     protected override void OnUpdate()
//     {
         
//     }


// }
