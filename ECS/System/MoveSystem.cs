using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using FixedMath;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(PathFind))]
[UpdateBefore(typeof(RVOSystem))]
public class MoveSystem : SystemBase
{
    public EndFixedStepSimulationEntityCommandBufferSystem endFixedStepSimulationEntityCommandBufferSystem;
    protected override void OnCreate()
    {
        endFixedStepSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndFixedStepSimulationEntityCommandBufferSystem>();
    }
    protected override void OnUpdate()
    {


            Entities.ForEach((Entity entity , DynamicBuffer<PathPosition> pathPositionBuffer,ref Agent agent, ref PathFollow pathFollow ) =>{
                if (pathFollow.pathIndex >= 0) {
                    // Has path to follow
                    PathPosition pathPosition = pathPositionBuffer[pathFollow.pathIndex];

                    Vector2 targetPosition = new Vector2(pathPosition.position.x, pathPosition.position.y);
                    Vector2 moveDir =  targetPosition - agent.position_;
                    if(RVOMath.absSq(moveDir) != 0)
                        moveDir = RVOMath.normalize(moveDir);
                    agent.prefVelocity_ = moveDir;

                    

                 

                    if (RVOMath.absSq(agent.position_ - targetPosition) >= 0 && RVOMath.abs(agent.position_ - targetPosition)< (FixedInt)0.1f ) {
                        // Next waypoint
                        pathFollow.pathIndex--;
                        EntityCommandBuffer ecb =  endFixedStepSimulationEntityCommandBufferSystem.CreateCommandBuffer();
                        ecb.SetComponent<PathFollow>(entity, pathFollow);;
                    }
                }
                else{
                    agent.prefVelocity_ = new Vector2(0,0);
                }
                
                
             
                

            }).WithoutBurst().Run();



 
        // else{
        //      Entities.ForEach((Entity entity ,ref Agent agent) =>{
              
             
        //         agent.prefVelocity_ = new Vector2(0,0);

        //     }).WithoutBurst().Run();
        // }
    }

     
}
