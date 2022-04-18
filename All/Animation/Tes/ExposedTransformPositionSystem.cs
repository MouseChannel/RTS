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

[BurstCompile]
public partial struct UpdateExposedTransformJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter ecbPara;
    void Execute([EntityInQueryIndex] int entityInQueryIndex, ref AnimationData animationData, in ExposedTransformSystemState exposedTransformSystemState, in Translation translation, in Rotation rotation)
    {
        ref var frameDatasRef = ref animationData.currentAnimation.Value;
        ref var frameDatas = ref frameDatasRef.exposedFramePositionData;
        ref var currentFrameData = ref frameDatas[animationData.currentFrame].singleFrameData;

        for (int i = 0; i < exposedTransformSystemState.count; i++)
        {
            var entity = exposedTransformSystemState.GetEntity(i);
            ecbPara.SetComponent<Translation>(entityInQueryIndex, entity, new Translation
            {
                Value = translation.Value + currentFrameData[0].translation
            });
            ecbPara.SetComponent<Rotation>(entityInQueryIndex, entity, new Rotation
            {
                Value = math.mul(rotation.Value, currentFrameData[0].rotation)
            });


        }

    }



}


[BurstCompile]
public partial struct CrossfadeTransformJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter ecbPara;

    void Execute([EntityInQueryIndex] int entityInQueryIndex, ref InCrossfade inCrossfade, ref AnimationData animationData, in ExposedTransformSystemState exposedTransformSystemState, in Translation translation, in Rotation rotation)
    {

    }


}


public partial class ExposedTransformPositionSystem : ServiceSystem
{
    private EntityQueryDesc dntNeedCrossfade = new EntityQueryDesc
    {
        None = new ComponentType[] { typeof(InCrossfade) }
    };
    protected override void OnStartRunning()
    {


        // worldTimeEntity = GetSingleton<>
    }


    protected override void OnUpdate()
    {
        var deltaTime = UnityEngine.Time.deltaTime;
        var UpdateFrameJobHanlde = new UpdateFrameJob
        {
            deltaTime = deltaTime,
            ecbPara = ecbPara



        }.ScheduleParallel(Dependency);
        Profiler.BeginSample("wefwef");
        var dontNeedCrossfadeQuery = GetEntityQuery(dntNeedCrossfade);
        Profiler.EndSample();
        new UpdateExposedTransformJob
        {
            ecbPara = ecbPara
        }.ScheduleParallel(UpdateFrameJobHanlde).Complete();


        // new CrossfadeTransformJob{

        // }.ScheduleParallel(GetEntityQuery(typeof))

    }



}


