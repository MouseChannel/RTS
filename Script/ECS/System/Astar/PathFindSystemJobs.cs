using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Jobs;
using FixedMath;
using System;
using Unity.Collections.LowLevel.Unsafe;

public partial class PathFindSystem
{
    private static readonly int gridWidth = ConfigData.gridWidth;
    private static readonly int gridLength = ConfigData.gridLength;

    public struct pair : IComparable<pair>, IEquatable<pair>
    {
        public int index;
        public int cost;


        public int CompareTo(pair other)
        {
            return other.cost - cost;
        }




        public bool Equals(pair other)
        {
            return index == other.index;
        }
    }

    [BurstCompile]
    private struct BuildPathGridCostJob : IJob 
    {
        [NativeDisableContainerSafetyRestriction]
        private DynamicBuffer<PathPosition> pathPositionBuffer;
        [DeallocateOnJobCompletion]
        public NativeArray<GridNode> pathNodeArray;
        public int startNodeIndex;
        public int endNodeIndex;

        public Entity entity;






        public int indexInQuery;
        [NativeDisableContainerSafetyRestriction]
        public EntityCommandBuffer.ParallelWriter ecbPara;







        public void Execute( )
        {


            BuildPathGridCost();

            CalculatePath();
            DeleteCollinear();
            RemoveCorner();

            if (pathPositionBuffer.Length > 0)
                ecbPara.SetComponent<CurrentPathIndex>(indexInQuery, entity, new CurrentPathIndex { pathIndex = pathPositionBuffer.Length - 2 });


        }

        private void BuildPathGridCost()
        {
            GridNode startNode = pathNodeArray[startNodeIndex];
            GridNode endNode = pathNodeArray[endNodeIndex];



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
            startNode.CalculateFCost();
            pathNodeArray[startNodeIndex] = startNode;


            NativeList<pair> openList = new NativeList<pair>(Allocator.Temp);

            NativeList<int> closedList = new NativeList<int>(Allocator.Temp);

            openList.Add(new pair { index = startNodeIndex, cost = startNode.fCost });

            while (openList.Length > 0)
            {
                int currentNodeIndex = PopLowestCostFNodeIndex(openList);
                GridNode currentNode = pathNodeArray[currentNodeIndex];

                if (currentNodeIndex == endNodeIndex)
                {
                    // Reached our destination!
                    break;
                }



                closedList.Add(currentNodeIndex);
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

                    int neighbourNodeIndex = CalculateIndex(neighbourPosition);

                    if (closedList.Contains(neighbourNodeIndex))
                    {
                        // Already searched this node
                        continue;
                    }

                    GridNode neighbourNode = pathNodeArray[neighbourNodeIndex];
                    if (!neighbourNode.isWalkable)
                    {
                        // Not walkable

                        continue;
                    }

                    int2 currentNodePosition = new int2(currentNode.x, currentNode.y);

                    int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNodePosition, new int2(endNode.x, endNode.y));
                    if (tentativeGCost < neighbourNode.gCost)
                    {
                        neighbourNode.cameFromNodeIndex = currentNodeIndex;
                        neighbourNode.gCost = tentativeGCost;
                        neighbourNode.CalculateFCost();

                        pathNodeArray[neighbourNodeIndex] = neighbourNode;
                        var neighbourPair = new pair { index = neighbourNodeIndex, cost = neighbourNode.fCost };

                        if (!openList.Contains(neighbourPair))
                        {

                            openList.Add(neighbourPair);
                        }
                    }

                }

            }

            neighbourOffsetArray.Dispose();
            openList.Dispose();
            closedList.Dispose();
        }


        private void CalculatePath()
        {
            pathPositionBuffer = ecbPara.AddBuffer<PathPosition>(indexInQuery, entity);
            var endNode = pathNodeArray[endNodeIndex];
            if (endNode.cameFromNodeIndex == -1)
            {
                // Couldn't find a path!
                ecbPara.SetComponent<CurrentPathIndex>(indexInQuery, entity, new CurrentPathIndex { pathIndex = -1 });
            }
            else
            {
                // Found a path


                pathPositionBuffer.Add(new PathPosition { position = new int2(endNode.x, endNode.y) });

                GridNode currentNode = endNode;
                while (currentNode.cameFromNodeIndex != -1)
                {
                    GridNode cameFromNode = pathNodeArray[currentNode.cameFromNodeIndex];
                    pathPositionBuffer.Add(new PathPosition { position = new int2(cameFromNode.x, cameFromNode.y) });
                    currentNode = cameFromNode;
                }
            }



        }



        private int CalculateIndex(int2 position)
        {
            return position.y * gridWidth + position.x;
        }
        private void DeleteCollinear()
        {
            NativeList<PathPosition> newPath = new NativeList<PathPosition>(Allocator.Temp);

            //除去共线的点
            if (pathPositionBuffer.Length <= 1) return;
            newPath.Add(pathPositionBuffer[0]);
            int2 dir = pathPositionBuffer[1].position - pathPositionBuffer[0].position;

            for (int i = 1; i < pathPositionBuffer.Length; i++)
            {
                int2 currentDir = pathPositionBuffer[i].position - pathPositionBuffer[i - 1].position;

                if (currentDir.x != dir.x || currentDir.y != dir.y)
                {

                    newPath.Add(pathPositionBuffer[i - 1]);
                    dir = currentDir;
                }
            }
            newPath.Add(pathPositionBuffer[pathPositionBuffer.Length - 1]);
            pathPositionBuffer.CopyFrom(newPath);


            newPath.Dispose();

        }

        private void RemoveCorner()
        {



            NativeList<PathPosition> newPath = new NativeList<PathPosition>(Allocator.Temp);
            newPath.Add(pathPositionBuffer[0]);
            int i = 0;
            int j = 2;
            while (j < pathPositionBuffer.Length)
            {

                if (IsExistBarrier(
                                    pathPositionBuffer[i].position,
                                    pathPositionBuffer[j].position))
                {
                    // Debug.Log(string.Format("exist barrier {0} {1} ||||{2} {3}",pathPositionBuffer[i].position.x,
                    //                                                             pathPositionBuffer[i].position.y,
                    //                                                             pathPositionBuffer[j].position.x,
                    //                                                             pathPositionBuffer[j].position.y)  );
                    newPath.Add(pathPositionBuffer[j - 1]);
                    // Debug.Log(string.Format("Add {0} {1}", pathPositionBuffer[j - 1].position.x, pathPositionBuffer[j - 1].position.y));
                    i = j - 1;

                }
                else
                {
                    j++;
                }
            }
            newPath.Add(pathPositionBuffer[pathPositionBuffer.Length - 1]);

            pathPositionBuffer.CopyFrom(newPath);





        }


        private int PopLowestCostFNodeIndex(NativeList<pair> openList)
        {

            openList.Sort();

            var result = openList[openList.Length - 1].index;
            openList.RemoveAtSwapBack(openList.Length - 1);
            return result;

        }

        private bool IsPositionInsideGrid(int2 gridPosition)
        {
            int y = gridPosition.y;
            int x = gridPosition.x;
            return x >= 0
                    && y >= 0
                    && x < gridWidth
                    && y < gridWidth;
        }

        private int CalculateDistanceCost(int2 aPosition, int2 bPosition)
        {
            int xDistance = math.abs(aPosition.x - bPosition.x);
            int yDistance = math.abs(aPosition.y - bPosition.y);
            int remaining = math.abs(xDistance - yDistance);

            return MOVE_DIAGONAL_COST * math.min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
        }
        private bool IsExistBarrier(int2 A, int2 B)
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
        private bool CheckWalkable(int x, int y) => pathNodeArray[y * gridWidth + x].isWalkable;

        private void CalulateLinerFunc(FixedMath.FixedVector2 A, FixedMath.FixedVector2 B, out FixedInt k, out FixedInt b)
        {
            k = (B.Y - A.Y) / (B.X - A.X);
            b = A.Y - k * A.X;
        }

    }

}
