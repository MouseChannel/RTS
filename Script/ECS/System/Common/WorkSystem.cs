using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

// [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public class WorkSystem : SystemBase
{
    protected EndSimulationEntityCommandBufferSystem endFixedStepSimulationEntityCommandBufferSystem;
    protected override void OnCreate()
    {
        endFixedStepSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
 
        // World.GetOrCreateSystem<ResponseCommandSystem>().workList.Add(this);
        // FightSystem.Instance.workList.Add(this);
    }
 
    public virtual void Work(){}

    protected override void OnUpdate()
    {
 
    }
}
 
