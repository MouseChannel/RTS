
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
        [ReadOnly]  public NativeArray<ObstacleVertice> obstacles_;
        [ReadOnly]  public NativeArray<ObstacleTreeNode> obstacleTree_;
        [ReadOnly] public ObstacleTreeNode obstacleTreeRoot;

        [ReadOnly] public FOWUnit fowUnit;
        public UnsafeHashSet<int>.ParallelWriter setParaWriter;


        public  void Execute()
        {

            int range = fowUnit.range;







            #region 获取障碍物
            NativeList<ObstacleVertice> obstacleNeighbors = new NativeList<ObstacleVertice>(Allocator.Temp);
            GetRangeObstacleVertices(fowUnit.position, obstacleTreeRoot, fowUnit.range * fowUnit.range, obstacleNeighbors);


            // }

                // for (int i = 0; i < obstacleNeighbors.Length; i++)
                // {
                //     Debug.Log(String.Format("{0} {1}  {2} {3}", obstacleNeighbors[i].point_, obstacleNeighbors[i].verticeId_, obstacleNeighbors[i].obstacleId_, obstacleNeighbors[i].direction_));
                // }


            #endregion
            // TestGizmos.InitGizmos();

            for (int i = -range; i <= range; i++)
                for (int j = -range; j <= range; j++)
                {
                    if (!CheckInRange(i, j, range)) continue;
                    var currentGridPos = fowUnit.position.ConvertToint2() + new int2(i, j);
                    if (!CheckUnVisiable(currentGridPos, obstacleNeighbors))
                    {
                        var index = GridSystem.GetGridIndexInFOW(currentGridPos);
                        // var index = GetIndexInMap(.ConvertToint2());
                        setParaWriter.Add(index);
                        // TestGizmos.SetGridUnvisiable(currentGridPos);

                    }

                }









            obstacleNeighbors.Dispose();





        }

        private int GetIndexInMap(int2 pos)
        {

            var index = fowUnit.position.ConvertToint2() + pos;
            if (index.x >= 0 && index.x < ConfigData.gridWidth && index.y >= 0 && index.y < ConfigData.gridWidth)
                return index.y * ConfigData.gridWidth + index.x;
            return 0;
        }

        /// <summary>
        /// return true, if the grid is unvisiable
        /// </summary>
        /// <param name="gridPosition"></param>
        /// <param name="obstacleNeighbors"></param>
        /// <returns></returns>
        private bool CheckUnVisiable(FixedVector2 gridPosition, NativeList<ObstacleVertice> obstacleNeighbors)
        {
            //从后往前遍历
            int from;
            int to;
            to = obstacleNeighbors.Length - 1;
            from = obstacleNeighbors.Length;
            while (to >=0 )
            {

                if (obstacleNeighbors[to].verticeId_ >= 0)
                {
                    to--;
                    continue;
                }
                NativeSlice<ObstacleVertice> tempOnstacle = new NativeSlice<ObstacleVertice>(obstacleNeighbors, to + 1, from - to - 1);


                if (!CheckSingleObstacleVisiable(gridPosition, tempOnstacle))
                {
                    return true;
                }
                from = to;

                to-- ;

            }
            return false;



        }

        private bool CheckSingleObstacleVisiable(FixedVector2 gridPosition, NativeSlice<ObstacleVertice> tempOnstacle)
        {

            for (int i = 0; i < tempOnstacle.Length ; i++)
            {
                if (CheckIntheSameLeftAsUnit(tempOnstacle[i].direction_, tempOnstacle[i].point_, gridPosition))
                {
                    return true;
                }
            }
            return false;
        }
        // private bool CheckIntheSameLeftAsUnit(FixedVector2 ObstacleVertice_a, FixedVector2 gridPos, FixedVector2 ObstacleVertice_b)
        // {
        //     return FixedCalculate.leftOf(ObstacleVertice_a, gridPos, ObstacleVertice_b).sign == FixedCalculate.leftOf(ObstacleVertice_a, fowUnit.position, ObstacleVertice_b).sign;

        // }
         private bool CheckIntheSameLeftAsUnit(FixedVector2 obstacleVerticeDir, FixedVector2 obstacleVerticePoint, FixedVector2 gridPos)
        {
            return FixedCalculate.det(obstacleVerticeDir, gridPos - obstacleVerticePoint).sign == FixedCalculate.det(obstacleVerticeDir, fowUnit.position - obstacleVerticePoint).sign;
            // return FixedCalculate.leftOf(ObstacleVertice_a, gridPos, ObstacleVertice_b).sign == FixedCalculate.leftOf(ObstacleVertice_a, fowUnit.position, ObstacleVertice_b).sign;

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


        private void GetRangeObstacleVertices(FixedVector2 fOWUnitPosition, ObstacleTreeNode node, FixedInt rangeSq, NativeList<ObstacleVertice> obstacleNeighbors)
        {
            if (node.obstacleIndex == -1) return;
            ObstacleVertice obstacle1 = obstacles_[node.obstacleIndex];
            ObstacleVertice obstacle2 = obstacles_[obstacle1.next_];

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

        private  void InsertObstacleNeighbor(FixedVector2 fOWUnitPosition, ObstacleVertice obstacle, NativeList<ObstacleVertice> obstacleNeighbors_, FixedInt rangeSq)
        {
            ObstacleVertice nextObstacle = obstacles_[obstacle.next_];

            FixedInt distSq = FixedCalculate.distSqPointLineSegment(obstacle.point_, nextObstacle.point_, fOWUnitPosition);


            if (distSq < rangeSq)
            {
                if(obstacleNeighbors_.Contains(obstacle)){
                   var index = obstacleNeighbors_.IndexOf(obstacle);

                    obstacleNeighbors_.Add(new ObstacleVertice());
                    for (int i = obstacleNeighbors_.Length - 1; i > index;i--){
                        obstacleNeighbors_[i] = obstacleNeighbors_[i - 1];
                    }
                    obstacleNeighbors_[index] = obstacle;
                }
                else{
                    obstacleNeighbors_.Add(new ObstacleVertice { verticeId_ = -2 });
                    obstacleNeighbors_.Add(obstacle);
                }

                        
                

                // if (obstacleNeighbors_.Length == 0 ||
                //    obstacleNeighbors_[obstacleNeighbors_.Length - 1] != obstacle)
                // {
                //     //用-2来分割不同的 obstacle块
                //     obstacleNeighbors_.Add(new Obstacle { id_ = -2 });
                //     obstacleNeighbors_.Add(obstacle);
                // }

                // obstacleNeighbors_.Add(nextObstacle);

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
