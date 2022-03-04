using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using RVO;
using Unity.Jobs;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using FixedMath;
using System;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]

public partial class FOWSystem : SystemBase
{
    public NativeArray<Color32> colorBuffer = new NativeArray<Color32>(GridSystem.Instance.GetLength(), Allocator.Persistent);
    public NativeArray<Color32> blurBuffer = new NativeArray<Color32>(GridSystem.Instance.GetLength(), Allocator.Persistent);
    public Material blurMat;
    private Texture2D texBuffer;
    private RenderTexture renderBuffer;
    private RenderTexture renderBuffer2;
    private RenderTexture nextTexture;
    private RenderTexture curTexture;
    /// <summary>
    /// 迷雾贴图对外接口
    /// </summary>
    /// <value></value>
    public Texture FogTexture
    {
        get
        {
            return curTexture;
        }
    }

    private KDTreeSystem kDTreeSystem;


    protected override void OnCreate()
    {
        World.GetOrCreateSystem<FixedStepSimulationSystemGroup>().Timestep = 0.5f;
        kDTreeSystem = World.GetOrCreateSystem<KDTreeSystem>();
    }
    protected override void OnDestroy()
    {
        colorBuffer.Dispose();
        blurBuffer.Dispose();

        RenderTexture.ReleaseTemporary(renderBuffer);
        RenderTexture.ReleaseTemporary(renderBuffer2);
        RenderTexture.ReleaseTemporary(nextTexture);
        RenderTexture.ReleaseTemporary(curTexture);

    }
    protected override void OnUpdate()
    {
        FreshFog();
        CalculateFog();



        texBuffer.SetPixels32(blurBuffer.ToArray());
        texBuffer.Apply();
        Graphics.Blit(texBuffer, curTexture, blurMat, 0);

        // Graphics.Blit(texBuffer, renderBuffer, blurMat, 0);
        // // for (int i = 0; i < 1; i++)
        // // {
        // Graphics.Blit(renderBuffer, renderBuffer2, blurMat, 0);
        // Graphics.Blit(renderBuffer2, renderBuffer, blurMat, 0);
        // // }
        // Graphics.Blit(renderBuffer, nextTexture);





        // Lerp();



        // var mapWidth = GridSystem.Instance.pathfindingGrid.GetWidth();

        // var mapHeight = GridSystem.Instance.pathfindingGrid.GetHeight();
        // for (int j = 0; j < mapHeight; j++)
        // {
        //     for (int i = 0; i < mapWidth; i++)
        //     {
        //         if (colorBuffer[i].a != 255)
        //         {
        //             Debug.Log("different");
        //         }
        //     }

        // }


    }

    private void CalculateFog()
    {
        NativeList<JobHandle> jobList = new NativeList<JobHandle>(Allocator.Temp);
        NativeArray<ObstacleVertice> obstacles_ = new NativeArray<ObstacleVertice>(kDTreeSystem.obstacles_, Allocator.TempJob);
        NativeArray<ObstacleTreeNode> obstacleTree_ = new NativeArray<ObstacleTreeNode>(kDTreeSystem.obstacleTree_, Allocator.TempJob);
        ObstacleTreeNode obstacleTreeRoot = kDTreeSystem.obstacleTreeRoot;


        UnsafeHashSet<int> visiableArea = new UnsafeHashSet<int>(colorBuffer.Length, Allocator.TempJob);
        var setParaWriter = visiableArea.AsParallelWriter();
        Entities.ForEach((in FOWUnit fogUnit) =>
        {
            ComputeFog computeFog = new ComputeFog
            {
                mapBlurBuffer = blurBuffer,
                fowUnit = fogUnit,
                obstacles_ = obstacles_,
                obstacleTree_ = obstacleTree_,
                obstacleTreeRoot = obstacleTreeRoot,
                setParaWriter = setParaWriter


            };
            // computeFog.Run();
            jobList.Add(computeFog.Schedule());

            #region  delete
            //---------






            //     var fowUnit = fogUnit;


            //     int range = fowUnit.range;







            //     #region 获取障碍物
            //     NativeList<ObstacleVertice> obstacleNeighbors = new NativeList<ObstacleVertice>(Allocator.Temp);
            //     GetRangeObstacleVertices(fowUnit.position, obstacleTreeRoot, fowUnit.range * fowUnit.range, obstacleNeighbors);


            //     // }

            //         for (int i = 0; i < obstacleNeighbors.Length; i++)
            //         {
            //             Debug.Log(String.Format("{0} {1}  {2}", obstacleNeighbors[i].point_, obstacleNeighbors[i].verticeId_, obstacleNeighbors[i].direction_));
            //         }


            //     #endregion
            //     TestGizmos.InitGizmos();

            //     for (int i = -range; i <= range; i++)
            //         for (int j = -range; j <= range; j++)
            //         {
            //             if (!CheckInRange(i, j, range)) continue;
            //             var currentGridPos = fowUnit.position.ConvertToint2() + new int2(i, j);
            //             if (!CheckUnVisiable(currentGridPos, obstacleNeighbors))
            //             {
            //                 var index = GridSystem.GetGridIndexInFOW(currentGridPos);
            //                 // var index = GetIndexInMap(.ConvertToint2());
            //                 setParaWriter.Add(index);
            //                 TestGizmos.SetGridUnvisiable(currentGridPos);

            //             }

            //         }





            //  int GetIndexInMap(int2 pos)
            // {

            //     var index = fowUnit.position.ConvertToint2() + pos;
            //     if (index.x >= 0 && index.x < ConfigData.gridWidth && index.y >= 0 && index.y < ConfigData.gridWidth)
            //         return index.y * ConfigData.gridWidth + index.x;
            //     return 0;
            // }

            // /// <summary>
            // /// return true, if the grid is unvisiable
            // /// </summary>
            // /// <param name="gridPosition"></param>
            // /// <param name="obstacleNeighbors"></param>
            // /// <returns></returns>
            //  bool CheckUnVisiable(FixedVector2 gridPosition, NativeList<ObstacleVertice> obstacleNeighbors)
            // {
            //     //从后往前遍历
            //     int from;
            //     int to;
            //     to = obstacleNeighbors.Length - 1;
            //     from = obstacleNeighbors.Length;
            //     while (to >=0 )
            //     {

            //         if (obstacleNeighbors[to].verticeId_ >= 0)
            //         {
            //             to--;
            //             continue;
            //         }
            //         NativeSlice<ObstacleVertice> tempOnstacle = new NativeSlice<ObstacleVertice>(obstacleNeighbors, to + 1, from - to - 1);


            //         if (!CheckSingleObstacleVisiable(gridPosition, tempOnstacle))
            //         {
            //             return true;
            //         }
            //         from = to;

            //         to-- ;

            //     }
            //     return false;



            // }

            // bool CheckSingleObstacleVisiable(FixedVector2 gridPosition, NativeSlice<ObstacleVertice> tempOnstacle)
            // {

            //     for (int i = 0; i < tempOnstacle.Length ; i++)
            //     {
            //         if (CheckIntheSameLeftAsUnit(tempOnstacle[i].direction_, tempOnstacle[i].point_, gridPosition))
            //         {
            //             return true;
            //         }
            //     }
            //     return false;
            // }
            // // private bool CheckIntheSameLeftAsUnit(FixedVector2 ObstacleVertice_a, FixedVector2 gridPos, FixedVector2 ObstacleVertice_b)
            // // {
            // //     return FixedCalculate.leftOf(ObstacleVertice_a, gridPos, ObstacleVertice_b).sign == FixedCalculate.leftOf(ObstacleVertice_a, fowUnit.position, ObstacleVertice_b).sign;

            // // }
            //  bool CheckIntheSameLeftAsUnit(FixedVector2 obstacleVerticeDir, FixedVector2 obstacleVerticePoint, FixedVector2 gridPos)
            // {
            //     return FixedCalculate.det(obstacleVerticeDir, gridPos - obstacleVerticePoint).sign == FixedCalculate.det(obstacleVerticeDir, fowUnit.position - obstacleVerticePoint).sign;
            //     // return FixedCalculate.leftOf(ObstacleVertice_a, gridPos, ObstacleVertice_b).sign == FixedCalculate.leftOf(ObstacleVertice_a, fowUnit.position, ObstacleVertice_b).sign;

            // }


            // /// <summary>
            // /// 
            // /// </summary>
            // /// <param name="x"></param>
            // /// <param name="y"></param>
            // /// <param name="range"></param>
            // /// <returns></returns>
            //   bool CheckInRange(int x, int y, int range) => x * x + y * y < range * range;
            //  int GetRangeIndex(int x, int y, int width) => y * width + x;

            // int GetMapIndex(int index, int rangeWidth)
            // {
            //     //暂时
            //     int mapWidth = 100;

            //     int startPos = GridSystem.GetGridIndexInFOW(fowUnit.position);
            //     var y = startPos / mapWidth;
            //     var x = startPos % mapWidth;

            //     var yy = index / rangeWidth;
            //     var xx = index % rangeWidth;

            //     var resulty = y + yy - rangeWidth / 2;
            //     var resultx = x + xx - rangeWidth / 2;


            //     if (resultx < 0 || resultx >= mapWidth || resulty < 0 || resulty >= mapWidth)
            //         return -1;


            //     return resulty * mapWidth + resultx;
            // }

            // Color32 ChangeColor(Color32 before, char mark, int value)
            // {
            //     switch (mark)
            //     {
            //         case 'r':
            //             before.r = (byte)value;

            //             break;
            //         case 'g':
            //             before.g = (byte)value;
            //             break;
            //         case 'b':
            //             before.b = (byte)value;
            //             break;
            //         case 'a':
            //             before.a = (byte)value;
            //             break;
            //     }
            //     return before;
            // }


            //  void GetRangeObstacleVertices(FixedVector2 fOWUnitPosition, ObstacleTreeNode node, FixedInt rangeSq, NativeList<ObstacleVertice> obstacleNeighbors)
            // {
            //     if (node.obstacleIndex == -1) return;
            //     ObstacleVertice obstacle1 = obstacles_[node.obstacleIndex];
            //     ObstacleVertice obstacle2 = obstacles_[obstacle1.next_];

            //     FixedInt agentLeftOfLine = FixedCalculate.leftOf(obstacle1.point_, obstacle2.point_, fOWUnitPosition);

            //     if (agentLeftOfLine >= 0)
            //     {
            //         if (node.left_index != -1) GetRangeObstacleVertices(fOWUnitPosition, obstacleTree_[node.left_index], rangeSq, obstacleNeighbors);
            //     }
            //     else
            //     {
            //         if (node.right_index != -1) GetRangeObstacleVertices(fOWUnitPosition, obstacleTree_[node.right_index], rangeSq, obstacleNeighbors);
            //     }
            //     // ComputeObstacleNeighbor(obstacles,obstacleTree, agentLeftOfLine >= 0 ? obstacleTree[node.left_index] : obstacleTree[node.right_index]  , agent, ref rangeSq, obstacleNeighbors);

            //     FixedInt distSqLine = FixedCalculate.sqr(agentLeftOfLine) / FixedCalculate.absSq(obstacle2.point_ - obstacle1.point_);

            //     if (distSqLine < rangeSq)
            //     {
            //         if (agentLeftOfLine < 0)
            //         {
            //             /*
            //                 * Try obstacle at this node only if agent is on right side of
            //                 * obstacle (and can see obstacle).
            //                 */
            //             InsertObstacleNeighbor(fOWUnitPosition, obstacle1, obstacleNeighbors, rangeSq);
            //             // agent.insertObstacleNeighbor(node.obstacle_, rangeSq);
            //         }

            //         /* Try other side of line. */
            //         if (agentLeftOfLine >= 0)
            //         {
            //             if (node.right_index != -1) GetRangeObstacleVertices(fOWUnitPosition, obstacleTree_[node.right_index], rangeSq, obstacleNeighbors);
            //         }
            //         else
            //         {
            //             if (node.left_index != -1) GetRangeObstacleVertices(fOWUnitPosition, obstacleTree_[node.left_index], rangeSq, obstacleNeighbors);
            //         }

            //     }

            // }

            // void InsertObstacleNeighbor(FixedVector2 fOWUnitPosition, ObstacleVertice obstacle, NativeList<ObstacleVertice> obstacleNeighbors_, FixedInt rangeSq)
            // {
            //     ObstacleVertice nextObstacle = obstacles_[obstacle.next_];

            //     FixedInt distSq = FixedCalculate.distSqPointLineSegment(obstacle.point_, nextObstacle.point_, fOWUnitPosition);


            //     if (distSq < rangeSq)
            //     {
            //         if(obstacleNeighbors_.Length == 0 || obstacleNeighbors_[obstacleNeighbors_.Length - 1].obstacleId_ != obstacle.obstacleId_)
            //             obstacleNeighbors_.Add(new ObstacleVertice { verticeId_ = -2 });
            //         obstacleNeighbors_.Add(obstacle);

            //         // if (obstacleNeighbors_.Length == 0 ||
            //         //    obstacleNeighbors_[obstacleNeighbors_.Length - 1] != obstacle)
            //         // {
            //         //     //用-2来分割不同的 obstacle块
            //         //     obstacleNeighbors_.Add(new Obstacle { id_ = -2 });
            //         //     obstacleNeighbors_.Add(obstacle);
            //         // }

            //         // obstacleNeighbors_.Add(nextObstacle);

            //     }

            // }









       




            //-------------
            #endregion


        }).WithoutBurst().Run();
        JobHandle.CompleteAll(jobList);

        foreach (var j in visiableArea)
        {
            blurBuffer[j] = new Color32(0, 0, 0, 0);
        }
                 obstacles_.Dispose();
                obstacleTree_.Dispose();

        jobList.Dispose();
        visiableArea.Dispose();


    }

    //----------



    //------------

    public void InitFOW()
    {
        var mapWidth = GridSystem.Instance.GetWidth();

        var mapHeight = GridSystem.Instance.GetWidth();
        // colorBuffer = new Color32[mapWidth * mapHeight];
        // blurBuffer = new Color32[mapWidth * mapHeight];

        blurMat = new Material(Shader.Find("ImageEffect/AverageBlur"));

        texBuffer = new Texture2D(mapWidth, mapHeight, TextureFormat.ARGB32, false);
        texBuffer.wrapMode = TextureWrapMode.Clamp;
        renderBuffer = RenderTexture.GetTemporary((int)(mapWidth * 1.5f), (int)(mapHeight * 1.5f), 0);
        renderBuffer2 = RenderTexture.GetTemporary((int)(mapWidth * 1.5f), (int)(mapHeight * 1.5f), 0);
        nextTexture = RenderTexture.GetTemporary((int)(mapWidth * 1.5f), (int)(mapHeight * 1.5f), 0);
        curTexture = RenderTexture.GetTemporary((int)(mapWidth * 1.5f), (int)(mapHeight * 1.5f), 0);
        for (int i = 0; i < blurBuffer.Length; i++)
        {
            blurBuffer[i] = new Color32(0, 0, 0, 255);

        }

        Debug.Log("InitFog");
    }

    private void FreshFog()
    {
        for (int i = 0; i < blurBuffer.Length; i++)
        {
            var color = blurBuffer[i];
            if (color.a == 0 || color.a == 120)
            {
                blurBuffer[i] = new Color32(0, 0, 0, 120);
            }
            else
            {
                blurBuffer[i] = new Color32(0, 0, 0, 255);
            }
            blurBuffer[i] = new Color32(0, 0, 0, 252);
            // if (color.r == 255)
            // {
            //     colorBuffer[i] = new Color32(0, color.g, color.b, color.a);
            // }


        }
    }
    private void Lerp()
    {
        Graphics.Blit(curTexture, renderBuffer);
        blurMat.SetTexture("_LastTex", renderBuffer);
        Graphics.Blit(nextTexture, curTexture, blurMat, 1);
    }



}
