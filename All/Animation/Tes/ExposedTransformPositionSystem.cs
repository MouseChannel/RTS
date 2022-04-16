using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine.Jobs;
using Unity.Mathematics;
using Unity.Burst;


[BurstCompile]
public partial struct UpdateFrameJob : IJobEntity
{
    public float deltaTime;

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







    }
}
public partial class ExposedTransformPositionSystem : ServiceSystem
{
    private Entity worldTimeEntity;
    protected override void OnStartRunning()
    {
        // worldTimeEntity = GetSingleton<>
    }


    protected override void OnUpdate()
    {
        var deltaTime = UnityEngine.Time.deltaTime;
        new UpdateFrameJob { deltaTime = deltaTime }.ScheduleParallel(Dependency).Complete();
        
    }



}


