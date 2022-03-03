
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using System.Collections.Generic;

using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;
using System;
using RVO;
using FixedMath;

public partial class FOWSystem
{
    [BurstCompile]
    public struct ComputeFog : IJob
    {
        [NativeDisableContainerSafetyRestriction]
        public NativeArray<Color32> mapBlurBuffer;
        [ReadOnly][DeallocateOnJobCompletion] public NativeArray<Obstacle> obstacles_;
        [ReadOnly][DeallocateOnJobCompletion] public NativeArray<ObstacleTreeNode> obstacleTree_;
        [ReadOnly] public ObstacleTreeNode obstacleTreeRoot;

        [ReadOnly] public FOWUnit fowUnit;
        public UnsafeHashSet<int2>.ParallelWriter setParaWriter;


        public    void Execute()
        {

            int range = fowUnit.range;
            // int rangeWidth = (range + 1) * 2 - 1;
            // int field = rangeWidth * rangeWidth;


            // NativeArray<Color32> colorBuffer = new NativeArray<Color32>(field, Allocator.Temp);
            // NativeArray<Color32> blurBuffer = new NativeArray<Color32>(field, Allocator.Temp);


            // for (int y = 0; y < rangeWidth; y++)
            // {
            //     for (int x = 0; x < rangeWidth; x++)
            //     {
            //         var index = GetRangeIndex(x, y, rangeWidth);
            //         if (!CheckInRange(x - range, y - range, range))
            //         {

            //             colorBuffer[index] = ChangeColor(colorBuffer[index], 'a', 255);
            //         }
            //         else
            //         {
            //             colorBuffer[index] = ChangeColor(colorBuffer[index], 'a', 0);
            //         }
            //     }
            // }






            #region 获取障碍物
            NativeList<Obstacle> obstacleNeighbors = new NativeList<Obstacle>(Allocator.Temp);
            GetRangeObstacleVertices(fowUnit.position, obstacleTreeRoot, fowUnit.range * fowUnit.range *111, obstacleNeighbors);
            if (obstacleNeighbors.Length > 0)
            {
                var lastObstacleVerticeIndex = obstacleNeighbors[obstacleNeighbors.Length - 1].next_;
                obstacleNeighbors.Add(obstacles_[lastObstacleVerticeIndex]);
            }

            // for (int i = 0; i < obstacleNeighbors.Length;i++){
            //     Debug.Log(String.Format("{0}", obstacleNeighbors[i].point_));
            // }

                #endregion


                // for (int y = -range; y <= range; y++)
                // {
                //     for (int x = -range; x < range; x++)
                //     {
                //         if (!CheckInRange(x, y, range)) continue;
                //         var currentGridPos = fowUnit.position + new FixedVector2(x, y);
                //         var indexInFOWMap = GridSystem.GetGridIndexInFOW(currentGridPos);
                //         if (CheckExposed(currentGridPos, obstacleNeighbors))
                //         {

                //             // if (mapBlurBuffer[indexInFOWMap].a > 0)
                //             mapBlurBuffer[indexInFOWMap] = ChangeColor(mapBlurBuffer[indexInFOWMap], 'a', 0);
                //         }

                //         // mapBlurBuffer[indexInFOWMap] = ChangeColor(mapBlurBuffer[indexInFOWMap], 'a', 0);

                //     }
                // }






            // for (int i = 0; i < obstacleNeighbors.Length; i++)
            // {
            //     Debug.Log(String.Format("{0}", obstacleNeighbors[i].point_));
            // }

            obstacleNeighbors.Dispose();






            #region 使探索过的区域变成半透明
            // for (int i = 0; i < colorBuffer.Length; i++)
            // {
            //     Color32 c = colorBuffer[i];
            //     if (c.r == 0)
            //     {

            //         blurBuffer[i] = ChangeColor(blurBuffer[i], 'a', c.b == 255 ?  120 :  255);
            //     }
            //     else
            //     {
            //         blurBuffer[i] = ChangeColor(blurBuffer[i], 'a', (byte)(255 - c.r));
            //     }
            // }



            #endregion


            // for (int i = 0; i < blurBuffer.Length; i++)
            // {

            //     var index = GetMapIndex(i, rangeWidth);

            //     if (index != -1
            //                 // && mapBlurBuffer[index].a > blurBuffer[i].a
            //                 )
            //     {
            //         mapBlurBuffer[index] = colorBuffer[i];
            //     }
            // }



            // colorBuffer.Dispose();
            // blurBuffer.Dispose();


        }

        private bool CheckExposed(FixedVector2 gridPosition, NativeList<Obstacle> obstacleNeighbors)
        {
            int index = 0;
            while(index < obstacleNeighbors.Length ){


            }

            for (int i = 0; i < obstacleNeighbors.Length - 1; i++)
            {
                
                NativeSlice<Obstacle> currentObstacleVertices = new NativeSlice<Obstacle>(obstacleNeighbors, 0, 4);
                if (!CheckInsideTriangle(obstacleNeighbors[i].point_, gridPosition, obstacleNeighbors[i + 1].point_))
                    return false;
            }
            return true;

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public bool CheckInRange(int x, int y, int range) => x * x + y * y < range * range;
        public int GetRangeIndex(int x, int y, int width) => y * width + x;

        public int GetMapIndex(int index, int rangeWidth)
        {
            //暂时
            int mapWidth = 100;

            int startPos = GridSystem.GetGridIndexInFOW(fowUnit.position);
            var y = startPos / mapWidth;
            var x = startPos % mapWidth;

            var yy = index / rangeWidth;
            var xx = index % rangeWidth;

            var resulty = y + yy - rangeWidth / 2;
            var resultx = x + xx - rangeWidth / 2;


            if (resultx < 0 || resultx >= mapWidth || resulty < 0 || resulty >= mapWidth)
                return -1;


            return resulty * mapWidth + resultx;
        }

        public Color32 ChangeColor(Color32 before, char mark, int value)
        {
            switch (mark)
            {
                case 'r':
                    before.r = (byte)value;

                    break;
                case 'g':
                    before.g = (byte)value;
                    break;
                case 'b':
                    before.b = (byte)value;
                    break;
                case 'a':
                    before.a = (byte)value;
                    break;
            }
            return before;
        }


        private void GetRangeObstacleVertices(FixedVector2 fOWUnitPosition, ObstacleTreeNode node, FixedInt rangeSq, NativeList<Obstacle> obstacleNeighbors)
        {
            if (node.obstacleIndex == -1) return;
            Obstacle obstacle1 = obstacles_[node.obstacleIndex];
            Obstacle obstacle2 = obstacles_[obstacle1.next_];

            FixedInt agentLeftOfLine = FixedCalculate.leftOf(obstacle1.point_, obstacle2.point_, fOWUnitPosition);

            if (agentLeftOfLine >= 0)
            {
                if (node.left_index != -1) GetRangeObstacleVertices(fOWUnitPosition, obstacleTree_[node.left_index], rangeSq, obstacleNeighbors);
            }
            else
            {
                if (node.right_index != -1) GetRangeObstacleVertices(fOWUnitPosition, obstacleTree_[node.right_index], rangeSq, obstacleNeighbors);
            }
            // ComputeObstacleNeighbor(obstacles,obstacleTree, agentLeftOfLine >= 0 ? obstacleTree[node.left_index] : obstacleTree[node.right_index]  , agent, ref rangeSq, obstacleNeighbors);

            FixedInt distSqLine = FixedCalculate.sqr(agentLeftOfLine) / FixedCalculate.absSq(obstacle2.point_ - obstacle1.point_);

            if (distSqLine < rangeSq)
            {
                if (agentLeftOfLine < 0)
                {
                    /*
                        * Try obstacle at this node only if agent is on right side of
                        * obstacle (and can see obstacle).
                        */
                    InsertObstacleNeighbor(fOWUnitPosition, obstacle1, obstacleNeighbors, rangeSq);
                    // agent.insertObstacleNeighbor(node.obstacle_, rangeSq);
                }

                /* Try other side of line. */
                if (agentLeftOfLine >= 0)
                {
                    if (node.right_index != -1) GetRangeObstacleVertices(fOWUnitPosition, obstacleTree_[node.right_index], rangeSq, obstacleNeighbors);
                }
                else
                {
                    if (node.left_index != -1) GetRangeObstacleVertices(fOWUnitPosition, obstacleTree_[node.left_index], rangeSq, obstacleNeighbors);
                }

            }

        }

        private void InsertObstacleNeighbor(FixedVector2 fOWUnitPosition, Obstacle obstacle, NativeList<Obstacle> obstacleNeighbors_, FixedInt rangeSq)
        {
            Obstacle nextObstacle = obstacles_[obstacle.next_];

            FixedInt distSq = FixedCalculate.distSqPointLineSegment(obstacle.point_, nextObstacle.point_, fOWUnitPosition);

            if (distSq < rangeSq)
            {
                obstacleNeighbors_.Add(obstacle);

                // int i = obstacleNeighbors_.Length - 1;

                // while (i != 0 && distSq < obstacleNeighbors_[i - 1].distance)
                // {
                //     obstacleNeighbors_[i] = obstacleNeighbors_[i - 1];
                //     --i;
                // }
                // obstacleNeighbors_[i] = new ObstacleNeighbor { distance = distSq, obstacle = obstacle };
            }

        }





        /// <summary>
        /// 
        ///                                   B  _____________                                             
        ///                                    *|             |                                
        ///                                 *###|             |                               
        ///                              *######|             |                              
        ///                           *#########|_____________|                                          
        ///                        * ######***** C                                        
        ///                     *  * * * *                                                   
        ///                   A                                                         
        ///
        /// 
        ///     图中#表示阴影 ,计算三点 A B C 形成的三角形，之间的阴影  
        /// ///     这里规定                                                                    
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <param name="C"></param>
        private bool CheckInsideTriangle(FixedVector2 A, FixedVector2 B, FixedVector2 C)
        {
            return FixedCalculate.leftOf(A, B, C).sign == FixedCalculate.leftOf(A, fowUnit.position, C).sign
                && FixedCalculate.leftOf(A, C, B).sign == FixedCalculate.leftOf(A, fowUnit.position, B).sign
                && FixedCalculate.leftOf(B, A, C).sign == FixedCalculate.leftOf(B, fowUnit.position, C).sign;
        }

        private void CalculateLinerFunc(int2 A, int2 B, out float k, out float b)
        {
            k = (B.y - A.y) / (B.x - A.x);
            b = A.y - k * A.x;
        }
    }













}
