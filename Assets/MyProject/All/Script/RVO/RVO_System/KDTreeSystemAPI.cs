using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using FixedMath;
using Unity.Collections;
using UnityEngine.Profiling;
using Unity.Jobs;

//  [UpdateInGroup(typeof(CommandGroup))]
//  [UpdateAfter(typeof(KeepWalkingSystem))]
[DisableAutoCreation]

public partial class KDTreeSystem : WorkSystem
{






    /// <summary>
    /// 每个逻辑帧运行， Agent的位置需要即时更新
    /// </summary>
    public override void Work()
    {




        agents_.Clear();
        agentTree_.Clear();





        Entities.ForEach((Entity entity, in Agent agent) =>
        {
            agents_.Add(agent);
            agentTree_.Add(new AgentTreeNode { });
            agentTree_.Add(new AgentTreeNode { });

        }).WithoutBurst().Run();






        new BuildAgentTreeJob
        {
            agents_ = agents_,
            agentTree_ = agentTree_
        }.Run();


        // new UpdateAgentJobParallel
        // {


        //     agents = agents_,
        //     agentTree = agentTree_,
        //     obstacleVertices_ = obstacleVertices_,
        //     obstacleVerticesTree_ = obstacleVerticesTree_,
        //     obstacleVerticesTreeRoot = obstacleVerticesTreeRoot,

        //     ecbPara = ecbPara

        // }.Schedule(agents_.Length, 4).Complete();


        new UpdateAgent
        {
            agents = agents_,
            agentTree = agentTree_,
            obstacleVertices_ = obstacleVertices_,
            obstacleVerticesTree_ = obstacleVerticesTree_,
            obstacleVerticesTreeRoot = obstacleVerticesTreeRoot,
            ecbPara = ecbPara,


        }.ScheduleParallel().Complete();










    }


    /// <summary>
    /// <para> 更新Obstacle_KDTree</para> 
    /// <para> 一般在 <see cref = "AddNewObstacle"/> 内调用   </para> 
    /// </summary>
    private void UpdateObstacleTree()
    {

        obstacles_.Clear();
        obstacleTree_.Clear();



        Entities.ForEach((Entity entity, in Obstacle obstacle) =>
        {
            obstacles_.Add(obstacle);
            obstacleTree_.Add(new ObstacleTreeNode { });
            obstacleTree_.Add(new ObstacleTreeNode { });

        }).WithoutBurst().Run();


        new BuildObstacleTreeJob
        {
            obstacles_ = obstacles_,
            obstacleTree_ =obstacleTree_

        }.Run();
        // BuildObstacleTree(0, obstacles_.Length, 0);

    }



    /// <summary>
    /// <para> 更新ObstacleVertice_KDTree</para> 
    /// <para> 一般在 <see cref = "AddNewObstacle"/> 内调用   </para> 
    /// </summary>
    private void UpdateObstacleVerticeTree()
    {
        obstacleVertices_.Clear();
        obstacleVerticesTree_.Clear();

        Entities.ForEach((Entity entity, DynamicBuffer<PreObstacleVertice> obstacleVertices) =>
        {
            ObstacleVerticeCollect(obstacleVertices_, obstacleVertices);
        }).WithoutBurst().Run();





        InitObstacleVerticeTree(obstacleVertices_.Length);
        NativeList<ObstacleVertice> currentObstacleVertices = new NativeList<ObstacleVertice>(Allocator.Temp);
        currentObstacleVertices.AddRange(obstacleVertices_);

        // BuildObstacleVerticeTreeJob buildObstacleVerticeTreeJob =
        // new BuildObstacleVerticeTreeJob
        // {
        //     obstacleVertices = obstacleVertices_,
        //     obstacleVerticesTree = obstacleVerticesTree_,
        //     obstacleVerticesTreeRoot = obstacleVerticesTreeRoot

        // };
        // buildObstacleVerticeTreeJob.Run();
        // obstacleVerticesTreeRoot = buildObstacleVerticeTreeJob.obstacleVerticesTreeRoot;
        obstacleVerticesTreeRoot = BuildObstacleVerticeTreeRecursive(currentObstacleVertices);


        currentObstacleVertices.Dispose();


    }

    /// <summary>
    /// 场景内增加障碍物是调用
    /// </summary>
    public void AddNewObstacle()
    {
        UpdateObstacleVerticeTree();
        UpdateObstacleTree();
    }




    #region  selection
    /// <summary>
    /// 根据输入坐标，在AgentKDTree上查找最近的Agent
    /// </summary>
    /// <param name="position">输入坐标</param>
    /// <param name="rangeSq">最小检查范围</param>
    /// <param name="agentNo">离输入坐标最近的AgentNo</param>
    /// <param name="node">从KD树的该下标开始搜索</param>
    public void GetClosestAgent(FixedVector2 position, ref FixedInt rangeSq, ref int agentNo, int node = 0)
    {


        if (agentTree_[node].end_ - agentTree_[node].begin_ <= MAX_LEAF_SIZE)
        {
            for (int i = agentTree_[node].begin_; i < agentTree_[node].end_; ++i)
            {

                FixedInt distSq = FixedCalculate.Square(position - agents_[i].position_);
                //Find EnemyUnit
                if (distSq < rangeSq)
                {
                    rangeSq = distSq;
                    agentNo = agents_[i].id_;
                }
            }
        }
        else
        {
            FixedInt distSqLeft = FixedCalculate.Square(FixedCalculate.Max(0, agentTree_[agentTree_[node].left_].minX_ - position.X)) + FixedCalculate.Square(FixedCalculate.Max(0, position.X - agentTree_[agentTree_[node].left_].maxX_)) + FixedCalculate.Square(FixedCalculate.Max(0, agentTree_[agentTree_[node].left_].minY_ - position.Y)) + FixedCalculate.Square(FixedCalculate.Max(0, position.Y - agentTree_[agentTree_[node].left_].maxY_));
            FixedInt distSqRight = FixedCalculate.Square(FixedCalculate.Max(0, agentTree_[agentTree_[node].right_].minX_ - position.X)) + FixedCalculate.Square(FixedCalculate.Max(0, position.X - agentTree_[agentTree_[node].right_].maxX_)) + FixedCalculate.Square(FixedCalculate.Max(0, agentTree_[agentTree_[node].right_].minY_ - position.Y)) + FixedCalculate.Square(FixedCalculate.Max(0, position.Y - agentTree_[agentTree_[node].right_].maxY_));

            if (distSqLeft < distSqRight)
            {
                if (distSqLeft < rangeSq)
                {
                    GetClosestAgent(position, ref rangeSq, ref agentNo, agentTree_[node].left_);

                    if (distSqRight < rangeSq)
                    {
                        GetClosestAgent(position, ref rangeSq, ref agentNo, agentTree_[node].right_);
                    }
                }
            }
            else
            {
                if (distSqRight < rangeSq)
                {
                    GetClosestAgent(position, ref rangeSq, ref agentNo, agentTree_[node].right_);

                    if (distSqLeft < rangeSq)
                    {
                        GetClosestAgent(position, ref rangeSq, ref agentNo, agentTree_[node].left_);
                    }
                }
            }

        }

    }

    /// <summary>
    /// 查找范围内的Agent
    /// </summary>
    /// <param name="position"></param>
    /// <param name="areaRect">xmin, xmax,ymin,  ymax</param>
    /// <param name="rangeSq"></param>
    /// <param name="node"></param>
    /// <param name="areaAgents"></param>
    public void GetAreaAgents(FixedVector2 position, Vector4 areaRect, FixedInt rangeSq, int node, List<int> areaAgents)
    {


        if (agentTree_[node].end_ - agentTree_[node].begin_ <= MAX_LEAF_SIZE)
        {
            for (int i = agentTree_[node].begin_; i < agentTree_[node].end_; ++i)
            {

                InsertAreaAgents(agents_[i], areaRect, areaAgents);

            }
        }
        else
        {
            FixedInt distSqLeft = FixedCalculate.Square(FixedCalculate.Max(0, agentTree_[agentTree_[node].left_].minX_ - position.X)) + FixedCalculate.Square(FixedCalculate.Max(0, position.X - agentTree_[agentTree_[node].left_].maxX_)) + FixedCalculate.Square(FixedCalculate.Max(0, agentTree_[agentTree_[node].left_].minY_ - position.Y)) + FixedCalculate.Square(FixedCalculate.Max(0, position.Y - agentTree_[agentTree_[node].left_].maxY_));
            FixedInt distSqRight = FixedCalculate.Square(FixedCalculate.Max(0, agentTree_[agentTree_[node].right_].minX_ - position.X)) + FixedCalculate.Square(FixedCalculate.Max(0, position.X - agentTree_[agentTree_[node].right_].maxX_)) + FixedCalculate.Square(FixedCalculate.Max(0, agentTree_[agentTree_[node].right_].minY_ - position.Y)) + FixedCalculate.Square(FixedCalculate.Max(0, position.Y - agentTree_[agentTree_[node].right_].maxY_));

            if (distSqLeft < distSqRight)
            {
                if (distSqLeft < rangeSq)
                {
                    GetAreaAgents(position, areaRect, rangeSq, agentTree_[node].left_, areaAgents);

                    if (distSqRight < rangeSq)
                    {
                        GetAreaAgents(position, areaRect, rangeSq, agentTree_[node].right_, areaAgents);
                    }
                }
            }
            else
            {
                if (distSqRight < rangeSq)
                {
                    GetAreaAgents(position, areaRect, rangeSq, agentTree_[node].right_, areaAgents);

                    if (distSqLeft < rangeSq)
                    {
                        GetAreaAgents(position, areaRect, rangeSq, agentTree_[node].left_, areaAgents);
                    }
                }
            }

        }

    }


    private void InsertAreaAgents(Agent agent, Vector4 areaRect, List<int> areaAgents)
    {

        var pos = agent.position_;
        bool Vaild(float a, float min, float max) => a > min && a < max;


        if (Vaild(pos.X.RawFloat, areaRect[0], areaRect[1]) &&
                    Vaild(pos.Y.RawFloat, areaRect[2], areaRect[3]))
        {
            areaAgents.Add(agent.id_);
        }
    }

    #endregion


    #region FOW Detect Obstacle





    private void GetRangeObstacleVertices(Agent agent, ObstacleVerticeTreeNode node, FixedInt rangeSq, NativeList<ObstacleVerticeNeighbor> obstacleNeighbors)
    {
        if (node.obstacleVertice_Index == -1) return;
        ObstacleVertice obstacle1 = obstacleVertices_[node.obstacleVertice_Index];
        ObstacleVertice obstacle2 = obstacleVertices_[obstacle1.next_];

        FixedInt agentLeftOfLine = FixedCalculate.LeftOf(obstacle1.point_, obstacle2.point_, agent.position_);

        if (agentLeftOfLine >= 0)
        {
            if (node.left_index != -1) GetRangeObstacleVertices(agent, obstacleVerticesTree_[node.left_index], rangeSq, obstacleNeighbors);
        }
        else
        {
            if (node.right_index != -1) GetRangeObstacleVertices(agent, obstacleVerticesTree_[node.right_index], rangeSq, obstacleNeighbors);
        }
        // ComputeObstacleNeighbor(obstacles,obstacleTree, agentLeftOfLine >= 0 ? obstacleTree[node.left_index] : obstacleTree[node.right_index]  , agent, ref rangeSq, obstacleNeighbors);

        FixedInt distSqLine = FixedCalculate.Square(agentLeftOfLine) / FixedCalculate.Square(obstacle2.point_ - obstacle1.point_);

        if (distSqLine < rangeSq)
        {
            if (agentLeftOfLine < 0)
            {
                /*
                    * Try obstacle at this node only if agent is on right side of
                    * obstacle (and can see obstacle).
                    */
                InsertObstacleNeighbor(agent, obstacle1, obstacleNeighbors, rangeSq);
                // agent.insertObstacleNeighbor(node.obstacle_, rangeSq);
            }

            /* Try other side of line. */
            if (agentLeftOfLine >= 0)
            {
                if (node.right_index != -1) GetRangeObstacleVertices(agent, obstacleVerticesTree_[node.right_index], rangeSq, obstacleNeighbors);
            }
            else
            {
                if (node.left_index != -1) GetRangeObstacleVertices(agent, obstacleVerticesTree_[node.left_index], rangeSq, obstacleNeighbors);
            }

        }

    }

    private void InsertObstacleNeighbor(Agent agent, ObstacleVertice obstacle, NativeList<ObstacleVerticeNeighbor> obstacleNeighbors_, FixedInt rangeSq)
    {
        ObstacleVertice nextObstacle = obstacleVertices_[obstacle.next_];

        FixedInt distSq = FixedCalculate.DistSqPointLineSegment(obstacle.point_, nextObstacle.point_, agent.position_);

        if (distSq < rangeSq)
        {
            obstacleNeighbors_.Add(new ObstacleVerticeNeighbor { distance = distSq, obstacle = obstacle });

            int i = obstacleNeighbors_.Length - 1;

            while (i != 0 && distSq < obstacleNeighbors_[i - 1].distance)
            {
                obstacleNeighbors_[i] = obstacleNeighbors_[i - 1];
                --i;
            }
            obstacleNeighbors_[i] = new ObstacleVerticeNeighbor { distance = distSq, obstacle = obstacle };
        }

    }

    #endregion






}
