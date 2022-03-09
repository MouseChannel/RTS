using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

// [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public abstract class WorkSystem : SystemBase
{
    public abstract void Work();
    // protected EndSimulationEntityCommandBufferSystem 
    protected EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBufferSystem;
    // protected BeginPresentationEntityCommandBufferSystem beginPresentationEntityCommandBufferSystem;

    protected override void OnCreate()
    {

        endSimulationEntityCommandBufferSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
        // beginPresentationEntityCommandBufferSystem = World.GetExistingSystem<BeginPresentationEntityCommandBufferSystem>();
        // World.GetOrCreateSystem<ResponseCommandSystem>().workList.Add(this);
        // FightSystem.Instance.workList.Add(this);
    }
    // public virtual void Init(){}



    protected override void OnUpdate()
    {

    }
}

