using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using FixedMath;

using Unity.Burst;
using Unity.Jobs;

using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;


// [UpdateInGroup(typeof(CommandGroup))]
// [UpdateAfter(typeof(PathFindSystem))]

 
/// <summary>
/// 连接A* 和 RVO
/// </summary>
public partial class KeepWalkingSystem : WorkSystem
{


    
    public override void Work()
    {

        Entities.WithAll<InhabitantComponent>()
                .ForEach((DynamicBuffer<PathPosition> pathPositionBuffer, ref Agent agent, ref CurrentPathIndex currentPathIndex) =>
        {



            if (currentPathIndex.pathIndex >= 0)
            {
                agent.reachDestination = false;

                PathPosition pathPosition = pathPositionBuffer[currentPathIndex.pathIndex];

                FixedVector2 targetPosition = new FixedVector2(pathPosition.position.x, pathPosition.position.y);
                FixedVector2 moveDir = targetPosition - agent.position_;
                if (FixedCalculate.Square(moveDir) != 0)
                    moveDir = FixedCalculate.Normalize(moveDir);
                agent.prefVelocity_ = moveDir;




                if (FixedCalculate.Square(agent.position_ - targetPosition) >= 0 && FixedCalculate.Abs(agent.position_ - targetPosition) < FixedInt.half)
                {
                    // Next waypoint
                    currentPathIndex.pathIndex--;
                }


            }
            else if (!agent.reachDestination)
            {
                agent.reachDestination = true;
                agent.prefVelocity_ = new FixedVector2(0, 0);
               
            }


        }).ScheduleParallel(this.Dependency).Complete();



    }

}
