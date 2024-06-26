using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using FixedMath;
using System;
using Unity.Collections;
using Unity.Jobs;

[UpdateInGroup(typeof(TaskGroup))]
public partial class CollectorSystem : WorkSystem
{
    private ResponseNetSystem responseNetSystem;
    public override void Work()
    {
        responseNetSystem = GetSystem<ResponseNetSystem>();





        var ecbPara = endSimulationEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        NativeList<JobHandle> jobList = new NativeList<JobHandle>(Allocator.Temp);
        Entities.WithAll<CollectorTag>()
                .ForEach((Entity entity, int entityInQueryIndex, in DoingCollect doingCollect, in Agent agent, in InhabitantComponent inhabitantComponent) =>
        {


            CollectorJob collectorJob = new CollectorJob
            {
                entity = entity,
                inhabitantComponent = inhabitantComponent,
                collectorPosition = agent.position_,

                resourcePosition = responseNetSystem.GetObstacleEntityPosition(doingCollect.resourceNo),
                stopPosition = GetStopPosition(doingCollect.resourceNo),

                ecbPara = ecbPara,
                entityInQueryIndex = entityInQueryIndex

            };
            jobList.Add(collectorJob.Schedule());





        }).WithoutBurst().Run();

        JobHandle.CompleteAll(jobList);
        jobList.Dispose();

    }



    // private int GetResourcePositionIndex(int id)
    // {
    //     var resourceEntity = responseNetSystem.GetObstacleEntity(id);
    //     var posIndex = -1;
    //     if (resourceEntity != Entity.Null)
    //     {
    //         posIndex = GetComponent<ResourceComponent>(resourceEntity).resourcePositionIndex;

    //     }
    //     return posIndex;
    // }
    private FixedVector2 GetStopPosition(int resourceId)
    {

        var resourceEntity = responseNetSystem.GetObstacleEntity(resourceId);
        var pos = FixedVector2.inVaild;
        if (resourceEntity != Entity.Null)
        {
            var stopNo = GetComponent<ResourceComponent>(resourceEntity).stopNo;
            pos = responseNetSystem.GetObstacleEntityPosition(stopNo);
        }

        return pos;
    }




}
