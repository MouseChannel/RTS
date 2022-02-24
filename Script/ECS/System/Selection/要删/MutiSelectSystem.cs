using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Jobs;

public struct NeedDestroy: IComponentData{}

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[DisableAutoCreation]
public class MutiSelectSystem : SystemBase
{
    private StepPhysicsWorld stepPhysicsWorld;
    private BuildPhysicsWorld buildPhysicsWorld;
    private EndFixedStepSimulationEntityCommandBufferSystem endFixedStepSimulationEntityCommandBufferSystem;
    private BeginPresentationEntityCommandBufferSystem beginPresentationEntityCommandBufferSystem;
    protected override void OnCreate()
    {

     
        stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        endFixedStepSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndFixedStepSimulationEntityCommandBufferSystem>();
        beginPresentationEntityCommandBufferSystem = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem>();
    }
    protected override void OnUpdate()
    {
        if(!HasSingleton<SelectionColliderTag>()) return ;
 
        var selectColliderEntity = GetSingletonEntity<SelectionColliderTag>();
        var ecb = endFixedStepSimulationEntityCommandBufferSystem.CreateCommandBuffer();
   
        JobHandle jobHandle = new SelectTriggerJob{

            ecb = ecb,
            selectColliderEntity = selectColliderEntity,
            canBeSelectedUnits = GetComponentDataFromEntity<CanBeSelected>()


        }.Schedule(stepPhysicsWorld.Simulation, ref buildPhysicsWorld.PhysicsWorld, Dependency);

        jobHandle.Complete();

        //delete selection collider
        if(HasComponent<NeedDestroy>(selectColliderEntity))
            ecb.DestroyEntity(selectColliderEntity);
        else    ecb.AddComponent<NeedDestroy>(selectColliderEntity, new NeedDestroy{});








    }

    public struct SelectTriggerJob : ITriggerEventsJob
    {
        public EntityCommandBuffer ecb;
        public Entity selectColliderEntity;
         
        public ComponentDataFromEntity<CanBeSelected> canBeSelectedUnits;
        public void Execute(TriggerEvent triggerEvent)
        {
            
            var a = triggerEvent.EntityA;
            var b = triggerEvent.EntityB;
 
          
            if(a != selectColliderEntity && b != selectColliderEntity) return;
            if(a != selectColliderEntity){
                //make sure a is selectCollider
                var temp = a;
                a = b;
                b = temp;
            }
            if(canBeSelectedUnits.HasComponent(b))
                ecb.AddComponent<SelectableEntityTag>(b);
           
            
 
        }
    }
}
