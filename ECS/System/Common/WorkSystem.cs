using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public class WorkSystem : SystemBase
{
    protected EndFixedStepSimulationEntityCommandBufferSystem endFixedStepSimulationEntityCommandBufferSystem;
    protected override void OnCreate()
    {
        endFixedStepSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndFixedStepSimulationEntityCommandBufferSystem>();
 
        World.GetOrCreateSystem<ResponseCommandSystem>().workList.Add(this);
    }
    public virtual void Work(){}

    protected override void OnUpdate()
    {
         
    }
}
 
