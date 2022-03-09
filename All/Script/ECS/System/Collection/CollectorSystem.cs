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
         var ecb  = endSimulationEntityCommandBufferSystem.CreateCommandBuffer() ;
        Entities.ForEach((Entity entity, ref Collector collector, ref Inhabitant inhabitant, in CollectCommand collectCommand) =>
        {
            Debug.Log("Collect Work");
            inhabitant.state = InhabitantState.Collect;
            collector.collectorState = CollectorState.idle;
            collector.resource = collectCommand.resource;
            ecb.RemoveComponent<CollectCommand>(entity);

        }).Run();


        var ecbPara = endSimulationEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        NativeList<JobHandle> jobList = new NativeList<JobHandle>(Allocator.Temp);
        Entities.ForEach((Entity entity, int entityInQueryIndex, in Inhabitant inhabitant, in Agent agent, in Collector collector) =>
        {
            if (inhabitant.state == InhabitantState.Collect)
            {
                CollectorJob collectorJob = new CollectorJob
                {
                    entity = entity,
                    
                    collector = collector,
                    collectorPosition = agent.position_,
                    
                    resourceEntity = GetEntity(collector.resource.resourceId),
                    resourcePositionIndex = collector.resource.resourcePositionIndex,
                    
                    stopEntity = GetEntity(collector.resource.stopId),
                    stopPositionIndex =  collector.resource.stopPositionIndex,
                    ecbPara = ecbPara,
                    entityInQueryIndex = entityInQueryIndex


                };
                jobList.Add(collectorJob.Schedule());
            }




        }).WithoutBurst().Run();

        JobHandle.CompleteAll(jobList);

    }


    private int GetObstaclePositionIndex(int id)
    {
        // var pos = ResponseNetSystem.Instance.allObstacle[id].
        return 1;
    }
    private Entity GetEntity(int id)
    {
        return ResponseNetSystem.Instance.allObstacle[id];
    }



}
