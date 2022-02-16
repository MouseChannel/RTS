using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public class CollectionSystem : JobComponentSystem
{
    private EndFixedStepSimulationEntityCommandBufferSystem endFixedStepSimulationEntityCommandBufferSystem;
    protected override void OnCreate(){
        endFixedStepSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndFixedStepSimulationEntityCommandBufferSystem>();
    }
 

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        JobHandle jobHandle = Entities.WithAll<NeedInteract>().
            ForEach((Entity entity, ref Collector collector) => {
            
        
        }).Schedule(inputDeps);





        endFixedStepSimulationEntityCommandBufferSystem.AddJobHandleForProducer(jobHandle);


        return jobHandle;
    }
}
