using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using FixedMath;
using System;
using Unity.Collections;
using Unity.Jobs;

public partial class CollectorSystem : WorkSystem
{
    public override void Work()
    {

        var ecb = endSimulationEntityCommandBufferSystem.CreateCommandBuffer();
 


        var ecbPara = endSimulationEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        NativeList<JobHandle> jobList = new NativeList<JobHandle>(Allocator.Temp);
        Entities.ForEach((Entity entity, int entityInQueryIndex, in DoingCollect collectCommand, in Agent agent, in Collector collector) =>
        {
            

            CollectorJob collectorJob = new CollectorJob
            {
                entity = entity,
                collector = collector,
                collectorPosition = agent.position_,

                resourcePositionIndex = GetResourcePositionIndex(collectCommand.resourceNo),
                stopPositionIndex = GetStopPositionIndex(collectCommand.resourceNo),

                ecbPara = ecbPara,
                entityInQueryIndex = entityInQueryIndex

            };
            jobList.Add(collectorJob.Schedule());





        }).WithoutBurst().Run();

        JobHandle.CompleteAll(jobList);

    }


    private int GetObstaclePositionIndex(int id)
    {
        // var pos = ResponseNetSystem.Instance.allObstacle[id].
        return 1;
    }
    private int GetResourcePositionIndex(int id)
    {
        var resourceEntity = GetSystem<ResponseNetSystem>().allObstacle[id];
        var posIndex = -1;
        if (resourceEntity != Entity.Null)
        {
            posIndex = GetComponent<ResourceComponent>(resourceEntity).resourcePositionIndex;

        }
        return posIndex;

    }
    private int GetStopPositionIndex(int resourceId)
    {
        var resourceEntity = GetSystem<ResponseNetSystem>().allObstacle[resourceId];

        var posIndex = -1;
        if (resourceEntity != Entity.Null)
        {
            var stopNo = GetComponent<ResourceComponent>(resourceEntity).stopNo;
            var stopEntity = GetSystem<ResponseNetSystem>().allObstacle[stopNo];
            if (stopEntity != Entity.Null)
            {
                posIndex = GetComponent<ResourceComponent>(stopEntity).stopPositionIndex;
            }

        }

        return posIndex;
    }




}
