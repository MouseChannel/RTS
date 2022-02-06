using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using RVO;
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public class SelectUtilSystem : SystemBase
{
    private SelectionSystem selectionSystem;
    private EndFixedStepSimulationEntityCommandBufferSystem endFixedStepSimulationEntityCommandBufferSystem;
    protected override void OnCreate()
    {
        selectionSystem = World.GetOrCreateSystem<SelectionSystem>();
        endFixedStepSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndFixedStepSimulationEntityCommandBufferSystem>();
    }
    protected override void OnUpdate()
    {
        var ecb = endFixedStepSimulationEntityCommandBufferSystem.CreateCommandBuffer();
        // Entities.ForEach((Entity entity, in SelectableEntityTag selectableEntityTag, in Agent agent)=>{
        //     selectionSystem.selectUnits.Add(agent.id_);
        //     ecb.RemoveComponent<SelectableEntityTag>(entity);
        // }).WithoutBurst().Run();
    }
}
