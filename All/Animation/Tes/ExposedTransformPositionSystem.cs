// using System.ComponentModel.DataAnnotations;
using System.Numerics;
// using System.Numerics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine.Jobs;
using Unity.Mathematics;
using Unity.Burst;
using UnityEngine.Profiling;
using Unity.Rendering;

[BurstCompile]
public partial struct UpdateFrameJob : IJobEntity
{
    public float deltaTime;
    public EntityCommandBuffer.ParallelWriter ecbPara;



    void Execute(ref AnimationData animationData)
    {
        var currentTime = animationData.currentTime + deltaTime;
        var animationLength = animationData.currentAnimation.Value.length;
        var totalFrames = animationData.currentAnimation.Value.totalFrames;

        animationData.currentTime =
        currentTime < animationLength ?
        currentTime : currentTime - animationLength;

        float normalizedTime = currentTime / animationLength;

        animationData.currentFrame = math.min((int)math.round(normalizedTime * totalFrames), totalFrames - 1);


        //

    }
}

// [BurstCompile]
// public partial struct UpdateExposedTransformJob : IJobEntity
// {
//     public EntityCommandBuffer.ParallelWriter ecbPara;
//     void Execute([EntityInQueryIndex] int entityInQueryIndex, ref AnimationData animationData, in ExposedTransformSystemState exposedTransformSystemState
//      , in Translation translation, in Rotation rotation
//      )
//     {
//         ref var frameDatasRef = ref animationData.currentAnimation.Value;
//         ref var frameDatas = ref frameDatasRef.exposedFramePositionData;
//         ref var currentFrameData = ref frameDatas[animationData.currentFrame].singleFrameData;

//         for (int i = 0; i < exposedTransformSystemState.count; i++)
//         {
//             var entity = exposedTransformSystemState.GetEntity(i);
//             Debug.Log(entity);
//             ecbPara.SetComponent<Translation>(entityInQueryIndex, entity, new Translation
//             {
//                 // Value = translation.Value + currentFrameData[0].translation
//                 Value = currentFrameData[0].translation
//             });
//             ecbPara.SetComponent<Rotation>(entityInQueryIndex, entity, new Rotation
//             {
//                 // Value = math.mul(rotation.Value, currentFrameData[0].rotation)
//                 Value = currentFrameData[0].rotation
//             });


//         }

//     }



// }


[BurstCompile]
public partial struct CrossfadeTransformJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter ecbPara;

    void Execute([EntityInQueryIndex] int entityInQueryIndex, ref InCrossfade inCrossfade, ref AnimationData animationData, in ExposedTransformSystemState exposedTransformSystemState, in Translation translation, in Rotation rotation)
    {

    }


}

public partial struct TestJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter ecbPara;
    // public UnitScriptableObject beSpawnedUnitSpriptableObject;
    void Execute([EntityInQueryIndex] int entityInQueryIndex, Entity e, in RenderMesh renderMesh, ref AnimationData animationData)
    {
        AnimationUtil.CrossfadeAnimation("walk", e, ref animationData, ecbPara, entityInQueryIndex);
        // Debug.Log("Start work");

    }
}
public partial struct TestJob1 : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter ecbPara;
    // public UnitScriptableObject beSpawnedUnitSpriptableObject;
    void Execute([EntityInQueryIndex] int entityInQueryIndex, Entity e, in RenderMesh renderMesh, ref AnimationData animationData)
    {
        AnimationUtil.CrossfadeAnimation("IdleManBored", e, ref animationData, ecbPara, entityInQueryIndex);


    }
}
public partial struct TestJob2 : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter ecbPara;
    // public UnitScriptableObject beSpawnedUnitSpriptableObject;
    void Execute([EntityInQueryIndex] int entityInQueryIndex, Entity e, in RenderMesh renderMesh, ref AnimationData animationData)
    {
        AnimationUtil.CrossfadeAnimation("MineWorking", e, ref animationData, ecbPara, entityInQueryIndex);


    }
}

public partial class ExposedTransformPositionSystem : ServiceSystem
{
    private EntityQueryDesc dntNeedCrossfade = new EntityQueryDesc
    {
        None = new ComponentType[] { typeof(InCrossfade) }
    };
    int sum = 0;
    protected override void OnStartRunning()
    {


        var beSpawnedUnitSpriptableObject = Resources.Load<UnitScriptableObject>("Unit");
        BlobAssetUtil.animationMesh.Add(1, beSpawnedUnitSpriptableObject.subMesh);
    }


    protected override void OnUpdate()
    {
        var deltaTime = UnityEngine.Time.deltaTime;
        // sum += 1;
        var intsum = (int)sum;
        StaticData.shaderTime = StaticData._shaderTime;

        var dontNeedCrossfadeQuery = GetEntityQuery(dntNeedCrossfade);
        var updateFrameJobHandle = new UpdateFrameJob
        {
            deltaTime = deltaTime,
            ecbPara = ecbPara

        }.Schedule();
        updateFrameJobHandle.Complete();
        if (intsum % 60 == 1)
        {
            new TestJob1
            {
                ecbPara = ecbPara,
                // beSpawnedUnitSpriptableObject = beSpawnedUnitSpriptableObject,
            }.Schedule().Complete();
        }
        else if (intsum % 60 == 30)
        {
            new TestJob
            {
                ecbPara = ecbPara,
                // beSpawnedUnitSpriptableObject = beSpawnedUnitSpriptableObject,
            }.Schedule().Complete();
        }

        if (Input.GetMouseButtonDown(1))
        {
            new TestJob
            {
                ecbPara = ecbPara,
                // beSpawnedUnitSpriptableObject = beSpawnedUnitSpriptableObject,
            }.Schedule().Complete();

        }
        else if (Input.GetKeyDown(KeyCode.F4))
        {
            new TestJob1
            {
                ecbPara = ecbPara,
                // beSpawnedUnitSpriptableObject = beSpawnedUnitSpriptableObject,
            }.Schedule().Complete();
        }
        else if (Input.GetKeyDown(KeyCode.F5))
        {
            new TestJob2
            {
                ecbPara = ecbPara,
                // beSpawnedUnitSpriptableObject = beSpawnedUnitSpriptableObject,
            }.Schedule().Complete();
        }


    }



}


