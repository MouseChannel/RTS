using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public abstract partial class ServiceSystem : SystemBase
{
    protected EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBufferSystem
    {
        get
        {
            if (e == null)
                e = World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
            return e;
        }
    }
    private EndSimulationEntityCommandBufferSystem e;

    protected EntityCommandBuffer ecb
    {
        get => endSimulationEntityCommandBufferSystem.CreateCommandBuffer();

    }
    protected EntityCommandBuffer.ParallelWriter ecbPara
    {
        get => endSimulationEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();

    }



    protected T GetSystem<T>() where T : SystemBase
    {
        return World.GetOrCreateSystemManaged<T>();
    }


}
