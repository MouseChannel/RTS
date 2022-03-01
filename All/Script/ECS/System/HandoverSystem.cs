using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using FixedMath;
using RVO;
using Unity.Burst;
using Unity.Jobs;

using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;


[DisableAutoCreation]
public class HandoverSystem : WorkSystem
{


    public override void Work()
    {

        Entities.ForEach((Entity entity, int entityInQueryIndex, DynamicBuffer<PathPosition> pathPositionBuffer, ref Agent agent, ref CurrentPathIndex currentPathIndex) =>
        {


            if (currentPathIndex.pathIndex >= 0)
            {

                PathPosition pathPosition = pathPositionBuffer[currentPathIndex.pathIndex];

                FixedVector2 targetPosition = new FixedVector2(pathPosition.position.x, pathPosition.position.y);
                FixedVector2 moveDir = targetPosition - agent.position_;
                if ( FixedCalculate.absSq(moveDir) != 0)
                    moveDir =  FixedCalculate.normalize(moveDir);
                agent.prefVelocity_ = moveDir;




                if ( FixedCalculate.absSq(agent.position_ - targetPosition) >= 0 &&  FixedCalculate.abs(agent.position_ - targetPosition) < FixedInt.half)
                {
                    // Next waypoint
                    currentPathIndex.pathIndex--;
                }


            }
            else
            {
                agent.prefVelocity_ = new FixedVector2(0, 0);
            }


        }).ScheduleParallel(this.Dependency).Complete();



    }
    // [BurstCompile]
    // public struct HandOverJob : IJob
    // {
    //     public CurrentPathIndex currentPathIndex;
    //     public Entity entity;
    //     public Agent agent;
    //     [NativeDisableContainerSafetyRestriction]
    //     public DynamicBuffer<PathPosition> pathPositionBuffer;
    //     // [DeallocateOnJobCompletion]
    //     // public NativeArray<PathPosition> pathPositionArray;
    //     public int indexInEntityQuery;
    //     [NativeDisableContainerSafetyRestriction]
    //     public EntityCommandBuffer.ParallelWriter ecbPara;


    //     public void Execute()
    //     {

    //         if (currentPathIndex.pathIndex >= 0)
    //         {
    //             // Has path to follow
    //             // Debug.Log(string.Format("{0} {1} ", agent.prefVelocity_.x_.ToString(), agent.prefVelocity_.y_.ToString()));

    //             PathPosition pathPosition = pathPositionBuffer[currentPathIndex.pathIndex];

    //             FixedVector2 targetPosition = new FixedVector2(pathPosition.position.x, pathPosition.position.y);
    //             FixedVector2 moveDir = targetPosition - agent.position_;
    //             if (RVOMath.absSq(moveDir) != 0)
    //                 moveDir = RVOMath.normalize(moveDir);
    //             agent.prefVelocity_ = moveDir;




    //             if (RVOMath.absSq(agent.position_ - targetPosition) >= 0 && RVOMath.abs(agent.position_ - targetPosition) < FixedInt.half)
    //             {
    //                 // Next waypoint
    //                 currentPathIndex.pathIndex--;
    //                 ecbPara.SetComponent<CurrentPathIndex>(indexInEntityQuery, entity, currentPathIndex); ;
    //             }


    //         }
    //         else
    //         {
    //             agent.prefVelocity_ = new FixedVector2(0, 0);
    //         }
    //         agent.closestEnemy_ = 1212;
    //         ecbPara.SetComponent<Agent>(indexInEntityQuery, entity, agent);
    //         // for (int i = 0; i < pathPositionBuffer.Length;i++){
    //         //     Debug.Log(string.Format("{0} ", pathPositionBuffer[i].position));
    //         // }
    //         for (int i = 0; i < 1000; i++)
    //         {
    //             var s = Mathf.Log10(Mathf.Exp(i));
    //         }

    //     }
    // }





}
