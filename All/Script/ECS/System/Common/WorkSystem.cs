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
    protected EntityCommandBuffer.ParallelWriter ecbPara
    {
        get
        {

            return World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>().CreateCommandBuffer().AsParallelWriter();

        }
    }


    protected override void OnCreate()
    {

        endSimulationEntityCommandBufferSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
    }
    // public virtual void Init(){}
    protected T GetSystem<T>() where T : SystemBase
    {
        return World.GetOrCreateSystem<T>();
    }



    protected override void OnUpdate()
    {

    }
}

