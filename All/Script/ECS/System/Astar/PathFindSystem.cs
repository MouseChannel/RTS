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
using UnityEngine.Profiling;

// [UpdateInGroup(typeof(CommandGroup))]
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
        Profiler.BeginSample("MyPathTest");
        // NativeArray<GridNode> pathNodeArray = new NativeArray<GridNode>(GridSystem.Instance.GetWidth() * GridSystem.Instance.GetWidth(), Allocator.TempJob);
        NativeArray<GridNode> pathNodeArray = GridSystem.Instance.GetGridArray();
        Profiler.EndSample();
        NativeArray<GridNode>.Copy(GridSystem.Instance.GetGridArray(), pathNodeArray);



        var ecbPara = endSimulationEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        List<BuildPathGridCostJob> testList = new List<BuildPathGridCostJob>();

        Entities.ForEach((Entity entity, int entityInQueryIndex, in PathFindParam pathfindParams, in Agent agent) =>
        {

            var startNodeIndex = GridSystem.GetGridIndex(agent.position_);

            if (startNodeIndex != pathfindParams.endPosition)
            {
                BuildPathGridCostJob buildPathGridCostJob = new BuildPathGridCostJob
                {


                    pathNodeArray = pathNodeArray,
                    startNodeIndex = startNodeIndex,
                    endNodeIndex = pathfindParams.endPosition,
                    entity = entity,
                    // pathPositionBuffer = pathPositionBuffer,
                    indexInQuery = entityInQueryIndex,
                    ecbPara = ecbPara,
                    gridWidth = 512,
                    MOVE_DIAGONAL_COST = 14,
                    MOVE_STRAIGHT_COST = 10


                    // ecb.AsParallelWriter()

                };
                buildPathGridCostJob.Run();
                // testList.Add(buildPathGridCostJob);
                // TestGiz(ttt);


                // jobHandleList.Add(buildPathGridCostJob.Schedule());

            }


            // ecb.RemoveComponent<PathFindParams>(entity);


        }).WithoutBurst().Run();


        JobHandle.CompleteAll(jobHandleList);
        //---

        foreach (var i in testList)
        {


            DynamicBuffer<PathPosition> pathPositionBuffer;


            // [NativeDisableContainerSafetyRestriction]
            // [DeallocateOnJobCompletion]
            // public NativeArray<GridNode> tempPathNodeArray;



            int startNodeIndex = i.startNodeIndex;
            int endNodeIndex = i.endNodeIndex;

            Entity entity = i.entity;






            int indexInQuery = i.indexInQuery;





            // NativeList<GridNode> tempPathNodeArray = new NativeList<GridNode>(Allocator.Temp);


            NativeList<GridNode> tempPathNodeArray = new NativeList<GridNode>(Allocator.Temp);
            NativeList<PathPosition> currentPathArray = new NativeList<PathPosition>(Allocator.Temp);

            NativeArray<GridNode> pathNodeArr = new NativeArray<GridNode>(pathNodeArray, Allocator.Temp);
            Profiler.BeginSample("BuildPathGridCost");

            BuildPathGridCost(tempPathNodeArray, pathNodeArr);
            Profiler.EndSample();
            // TestGiz(tempPathNodeArray);

            // // Profiler.BeginSample("BuildTest");
            Profiler.BeginSample("Smooth");
            CalculatePath(tempPathNodeArray, currentPathArray, pathNodeArr);
            // Profiler.EndSample();
            DeleteCollinear(currentPathArray);
            RemoveCorner(currentPathArray);
            Profiler.EndSample();

            if (currentPathArray.Length > 0)
            {
                pathPositionBuffer = ecbPara.SetBuffer<PathPosition>(indexInQuery, entity);
                pathPositionBuffer.CopyFrom(currentPathArray);
                ecbPara.SetComponent<CurrentPathIndex>(indexInQuery, entity, new CurrentPathIndex { pathIndex = pathPositionBuffer.Length - 2 });
            }
            //     ecbPara.SetComponent<CurrentPathIndex>(indexInQuery, entity, new CurrentPathIndex { pathIndex = pathPositionBuffer.Length - 2 });

            tempPathNodeArray.Dispose();
            currentPathArray.Dispose();
            pathNodeArr.Dispose();
            ecbPara.RemoveComponent<PathFindParam>(indexInQuery, entity);


            void TestGiz(NativeList<GridNode> tempPathNodeArray)
            {
                // Debug.Log(string.Format("{0}", tempPathNodeArray.Length));
                for (int i = 0; i < tempPathNodeArray.Length; i++)
                {
                    // GridSystem.SetGrid(tempPathNodeArray[i].index / 512, tempPathNodeArray[i].index % 512);
                    // Debug.Log(string.Format("{0} {1}",tempPathNodeArray[i].index / 512, tempPathNodeArray[i].index % 512));
                }

            }



            void BuildPathGridCost(NativeList<GridNode> tempPathNodeArray, NativeArray<GridNode> pathNodeArr)
            {
                GridNode startNode = pathNodeArr[startNodeIndex];
                GridNode endNode = pathNodeArr[endNodeIndex];



                NativeArray<int2> neighbourOffsetArray = new NativeArray<int2>(8, Allocator.Temp);






                neighbourOffsetArray[0] = new int2(-1, 0); // Left
                neighbourOffsetArray[1] = new int2(+1, 0); // Right
                neighbourOffsetArray[2] = new int2(0, +1); // Up
                neighbourOffsetArray[3] = new int2(0, -1); // Down
                neighbourOffsetArray[4] = new int2(-1, -1); // Left Down
                neighbourOffsetArray[5] = new int2(-1, +1); // Left Up
                neighbourOffsetArray[6] = new int2(+1, -1); // Right Down
                neighbourOffsetArray[7] = new int2(+1, +1); // Right Up


                startNode.gCost = 0;

                // pathNodeArray[startNodeIndex] = startNode;


                // NativeList<pair> openList = new NativeList<pair>(Allocator.Temp);
                NativeList<GridNode> openList = new NativeList<GridNode>(Allocator.Temp);

                NativeList<GridNode> closedList = new NativeList<GridNode>(Allocator.Temp);

                // openList.Add(new pair { index = startNodeIndex, cost = startNode.fCost });
                openList.Add(startNode);
                tempPathNodeArray.Add(startNode);

                while (openList.Length > 0)
                {
                    // int currentNodeIndex = PopLowestCostFNodeIndex(openList);
                    var currentNode = PopLowestCostFNode(openList);
                    // GridNode currentNode = tempPathNodeArray[currentNodeIndex];

                    if (currentNode.index == endNodeIndex)
                    {
                        // Reached our destination!
                        tempPathNodeArray.Add(currentNode);
                        break;
                    }



                    closedList.Add(currentNode);
                    // List<int> aaaa = new List<int>();


                    for (int i = 0; i < neighbourOffsetArray.Length; i++)
                    {
                        int2 neighbourOffset = neighbourOffsetArray[i];
                        // int2 neighbourPosition = new int2(currentNode.x + neighbourOffset.x, currentNode.y + neighbourOffset.y);

                        int2 neighbourPosition = new int2(currentNode.x + neighbourOffset.x, currentNode.y + neighbourOffset.y);


                        if (!IsPositionInsideGrid(neighbourPosition))
                        {
                            // Neighbour not valid position
                            continue;
                        }
                        var neighbourNode = pathNodeArr[CalculateIndex(neighbourPosition)];

                        // int neighbourNodeIndex = CalculateIndex(neighbourPosition);

                        if (closedList.Contains(neighbourNode))
                        {
                            // Already searched this node
                            continue;
                        }

                        // GridNode neighbourNode = tempPathNodeArray[neighbourNodeIndex];
                        if (!neighbourNode.isWalkable)
                        {
                            // Not walkable

                            continue;
                        }

                        int2 currentNodePosition = new int2(currentNode.x, currentNode.y);

                        int tentativeGCost = currentNode.gCost + CalculateDistanceCost(neighbourPosition, currentNodePosition);
                        if (tentativeGCost < neighbourNode.gCost)
                        {

                            // neighbourNode.cameFromNodeIndex = tempPathNodeArray.IndexOf(currentNode);
                            neighbourNode.cameFromNodeIndex = CalculateIndex(currentNodePosition);
                            neighbourNode.gCost = tentativeGCost;
                            neighbourNode.hCost = CalculateDistanceCost(neighbourPosition, new int2(endNode.x, endNode.y));
                            neighbourNode.fCost = neighbourNode.gCost + neighbourNode.hCost;

                            //---
                            // var newNode = new GridNode
                            // {
                            //     cameFromNodeIndex = currentNodeIndex,
                            //     fCost = tentativeGCost,

                            // };

                            //---
                            var neighbourNodeIndex = CalculateIndex(neighbourPosition);
                            pathNodeArr[neighbourNodeIndex] = neighbourNode;
                            // tempPathNodeArray[neighbourNodeIndex] = neighbourNode;
                            // var neighbourPair = new pair { index = neighbourNodeIndex, cost = neighbourNode.fCost };

                            if (!openList.Contains(neighbourNode))
                            {

                                openList.Add(neighbourNode);
                                tempPathNodeArray.Add(neighbourNode);


                            }
                        }

                    }

                }

                neighbourOffsetArray.Dispose();
                openList.Dispose();
                closedList.Dispose();
            }


            void CalculatePath(NativeList<GridNode> tempPathNodeArray, NativeList<PathPosition> currentPathArray, NativeArray<GridNode> pathNodeArr)
            {
                // pathPositionBuffer = ecbPara.SetBuffer<PathPosition>(indexInQuery, entity);
                var endNode = tempPathNodeArray[tempPathNodeArray.Length - 1];
                if (endNode.cameFromNodeIndex == -1)
                {
                    // Couldn't find a path!
                    ecbPara.SetComponent<CurrentPathIndex>(indexInQuery, entity, new CurrentPathIndex { pathIndex = -1 });
                }
                else
                {
                    // Found a path
                    currentPathArray.Add(new PathPosition { position = new int2(endNode.x, endNode.y) });

                    // pathPositionBuffer.Add(new PathPosition { position = new int2(endNode.x, endNode.y) });

                    GridNode currentNode = endNode;
                    while (currentNode.cameFromNodeIndex != -1)
                    {
                        GridNode cameFromNode = pathNodeArr[currentNode.cameFromNodeIndex];
                        currentPathArray.Add(new PathPosition { position = new int2(cameFromNode.x, cameFromNode.y) });
                        // pathPositionBuffer.Add(new PathPosition { position = new int2(cameFromNode.x, cameFromNode.y) });
                        currentNode = cameFromNode;
                    }
                }



            }



            int CalculateIndex(int2 position)
            {
                return position.y * gridWidth + position.x;
            }
            void DeleteCollinear(NativeList<PathPosition> currentPathArray)
            {
                NativeList<PathPosition> newPath = new NativeList<PathPosition>(Allocator.Temp);

                //除去共线的点
                if (currentPathArray.Length <= 1) return;
                newPath.Add(currentPathArray[0]);
                int2 dir = currentPathArray[1].position - currentPathArray[0].position;

                for (int i = 1; i < currentPathArray.Length; i++)
                {
                    int2 currentDir = currentPathArray[i].position - currentPathArray[i - 1].position;

                    if (currentDir.x != dir.x || currentDir.y != dir.y)
                    {

                        newPath.Add(currentPathArray[i - 1]);
                        dir = currentDir;
                    }
                }
                newPath.Add(currentPathArray[currentPathArray.Length - 1]);
                currentPathArray.Clear();
                currentPathArray.AddRange(newPath);

                // pathPositionBuffer.CopyFrom(newPath);


                newPath.Dispose();

            }

            void RemoveCorner(NativeList<PathPosition> currentPathArray)
            {



                NativeList<PathPosition> newPath = new NativeList<PathPosition>(Allocator.Temp);
                newPath.Add(currentPathArray[0]);
                int i = 0;
                int j = 2;
                while (j < currentPathArray.Length)
                {

                    if (IsExistBarrier(
                                        currentPathArray[i].position,
                                        currentPathArray[j].position))
                    {
                        // Debug.Log(string.Format("exist barrier {0} {1} ||||{2} {3}",currentPathArray[i].position.x,
                        //                                                             currentPathArray[i].position.y,
                        //                                                             currentPathArray[j].position.x,
                        //                                                             currentPathArray[j].position.y)  );
                        newPath.Add(currentPathArray[j - 1]);
                        // Debug.Log(string.Format("Add {0} {1}", currentPathArray[j - 1].position.x, currentPathArray[j - 1].position.y));
                        i = j - 1;

                    }
                    else
                    {
                        j++;
                    }
                }
                newPath.Add(currentPathArray[currentPathArray.Length - 1]);
                currentPathArray.Clear();
                currentPathArray.AddRange(newPath);

                // pathPositionBuffer.CopyFrom(newPath);





            }


            int PopLowestCostFNodeIndex(NativeList<pair> openList)
            {

                openList.Sort();

                var result = openList[openList.Length - 1].index;
                openList.RemoveAtSwapBack(openList.Length - 1);
                return result;

            }
            GridNode PopLowestCostFNode(NativeList<GridNode> openList)
            {

                openList.Sort();
                //native容器不保证顺序
                var result = openList[openList.Length - 1];


                openList.RemoveAtSwapBack(openList.Length - 1);


                return result;

            }

            bool IsPositionInsideGrid(int2 gridPosition)
            {
                int y = gridPosition.y;
                int x = gridPosition.x;
                return x >= 0
                        && y >= 0
                        && x < gridWidth
                        && y < gridWidth;
            }

            int CalculateDistanceCost(int2 aPosition, int2 bPosition)
            {
                int xDistance = math.abs(aPosition.x - bPosition.x);
                int yDistance = math.abs(aPosition.y - bPosition.y);
                int remaining = math.abs(xDistance - yDistance);
                // return xDistance + yDistance;

                return MOVE_DIAGONAL_COST * math.min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
            }
            bool IsExistBarrier(int2 A, int2 B)
            {

                //   Debug.Log(string.Format("Check barrier {0} {1} |{2} {3}", A.x,A.y,B.x,B.y));
                bool noBarrier;

                //先特殊处理k = 0和 k = 无穷两种情况，即 x = x and y = y
                if (A.x == B.x)
                {
                    for (int y = math.min(A.y, B.y); y < math.max(B.y, A.y); y++)
                    {
                        noBarrier = CheckWalkable(A.x, y);
                        if (!noBarrier) return true;

                    }
                    return false;
                }
                if (A.y == B.y)
                {
                    for (int x = math.min(A.x, B.x); x < math.max(A.x, B.x); x++)
                    {
                        noBarrier = CheckWalkable(x, A.y);
                        if (!noBarrier) return true;

                    }
                    return false;
                }
                void Swap<T>(ref T a, ref T b)
                {
                    T c = a;
                    a = b;
                    b = c;
                }

                if (A.x > B.x)
                {
                    Swap(ref A, ref B);
                }
                CalulateLinerFunc(new FixedMath.FixedVector2(A), new FixedMath.FixedVector2(B), out FixedInt k, out FixedInt b);



                if (FixedCalculate.Abs(k) == 1)
                {
                    if (k == 1)
                    {
                        for (int2 x = A; x.x < B.x; x++)
                        {
                            noBarrier = CheckWalkable(x.x, x.y);
                            if (!noBarrier) return true;
                        }
                    }
                    else
                    {
                        for (int2 x = A; x.x > B.x; x += new int2(1, -1))
                        {
                            noBarrier = CheckWalkable(x.x, x.y);
                            if (!noBarrier) return true;
                        }
                    }



                }
                else
                {
                    void UpdateIndex(ref FixedInt index) { if (k > 0) index++; else index--; }
                    bool checkIndex(FixedInt a, FixedInt b) => k > 0 ? a < b : a > b;

                    if (FixedCalculate.Abs(k) < 1)
                    {
                        for (FixedInt x = A.x + FixedInt.half; x < B.x; x++)
                        {
                            FixedInt y = k * x + b;

                            noBarrier = CheckWalkable((x - FixedInt.half).RawInt, y.round.RawInt)
                                        && CheckWalkable((x + FixedInt.half).RawInt, y.round.RawInt);
                            if (!noBarrier) return true;

                        }
                    }
                    else if (FixedCalculate.Abs(k) > 1)
                    {

                        for (FixedInt y = A.y + FixedInt.half; checkIndex(y, B.y); UpdateIndex(ref y))
                        {
                            FixedInt x = (y - b) / k;

                            noBarrier = CheckWalkable(x.round.RawInt, (y + FixedInt.half).RawInt)
                                        && CheckWalkable(x.round.RawInt, (y - FixedInt.half).RawInt);
                            if (!noBarrier) return true;

                        }

                    }
                }
                return false;


            }
            bool CheckWalkable(int x, int y) => pathNodeArray[y * gridWidth + x].isWalkable;

            void CalulateLinerFunc(FixedMath.FixedVector2 A, FixedMath.FixedVector2 B, out FixedInt k, out FixedInt b)
            {
                k = (B.Y - A.Y) / (B.X - A.X);
                b = A.Y - k * A.X;
            }





        }
        //-----







        jobHandleList.Dispose();



    }
    private void TestGiz(NativeList<GridNode> tempPathNodeArray)
    {
        Debug.Log(string.Format("{0}", tempPathNodeArray.Length));
        for (int i = 0; i < tempPathNodeArray.Length; i++)
        {
            GridSystem.SetGrid(tempPathNodeArray[i].index / 512, tempPathNodeArray[i].index % 512);
            // Debug.Log(string.Format("{0} {1}",tempPathNodeArray[i].index / 512, tempPathNodeArray[i].index % 512));
        }

    }



}
