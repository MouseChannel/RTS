using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixedMath;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using System;

using Unity.Collections.LowLevel.Unsafe;
using Unity.Burst;
using Unity.Jobs;
using UnityEngine.UI;

[DisableAutoCreation]
public partial class PathFindSystem : WorkSystem
{
    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;


    public override void Work()
    {

        if (!ShouldRunSystem()) return;
        int gridWidth = GridSystem.Instance.GetWidth();
        int gridHeight = GridSystem.Instance.GetWidth();
        int2 gridSize = new int2(gridWidth, gridHeight);


        List<BuildPathGridCostJob> buildPathGridCostJobList = new List<BuildPathGridCostJob>();

        NativeList<JobHandle> jobHandleList = new NativeList<JobHandle>(Allocator.Temp);
        // var jobWriter = jobHandleList.AsParallelWriter();
        NativeArray<GridNode> pathNodeArray = new NativeArray<GridNode>(GridSystem.Instance.GetWidth() * GridSystem.Instance.GetWidth(), Allocator.TempJob);

        NativeArray<GridNode>.Copy(GridSystem.Instance.GetGridArray(), pathNodeArray);



        var ecbPara = endSimulationEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        // DynamicBuffer<PathPosition> pathPositionBuffer,
        // var gridSystem = GridSystem.Instance;

        Entities.ForEach((Entity entity, int entityInQueryIndex,  in PathFindParam pathfindParams,  in Agent agent) =>
        {
            
            var startNodeIndex = GridSystem.GetGridIndex(agent.position_);
            BuildPathGridCostJob buildPathGridCostJob = new BuildPathGridCostJob
            {


                pathNodeArray = pathNodeArray,
                startNodeIndex = startNodeIndex,
                endNodeIndex = pathfindParams.endPosition,
                entity = entity,
                // pathPositionBuffer = pathPositionBuffer,
                indexInQuery = entityInQueryIndex,
                ecbPara = ecbPara
                // ecb.AsParallelWriter()

            };

            jobHandleList.Add(buildPathGridCostJob.Schedule());


            // ecb.RemoveComponent<PathFindParams>(entity);


        }).WithoutBurst().Run();
        // endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(this.Dependency);

        JobHandle.CompleteAll(jobHandleList);



        pathNodeArray.Dispose();
        jobHandleList.Dispose();



    }



}
