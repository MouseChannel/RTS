using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using FixedMath;
using Unity.Collections;


public partial class KDTreeSystem
{


    public void AddNewObstacle()
    {
        UpdateObstacleVerticeTree();
        UpdateObstacleTree();
    }




    #region  selection
    public void GetClosestAgent(FixedVector2 position, ref FixedInt rangeSq, ref int agentNo, int node)
    {


        if (agentTree_[node].end_ - agentTree_[node].begin_ <= MAX_LEAF_SIZE)
        {
            for (int i = agentTree_[node].begin_; i < agentTree_[node].end_; ++i)
            {

                FixedInt distSq = FixedCalculate.absSq(position - agents_[i].position_);
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
            FixedInt distSqLeft = FixedCalculate.sqr(FixedCalculate.Max(0, agentTree_[agentTree_[node].left_].minX_ - position.X)) + FixedCalculate.sqr(FixedCalculate.Max(0, position.X - agentTree_[agentTree_[node].left_].maxX_)) + FixedCalculate.sqr(FixedCalculate.Max(0, agentTree_[agentTree_[node].left_].minY_ - position.Y)) + FixedCalculate.sqr(FixedCalculate.Max(0, position.Y - agentTree_[agentTree_[node].left_].maxY_));
            FixedInt distSqRight = FixedCalculate.sqr(FixedCalculate.Max(0, agentTree_[agentTree_[node].right_].minX_ - position.X)) + FixedCalculate.sqr(FixedCalculate.Max(0, position.X - agentTree_[agentTree_[node].right_].maxX_)) + FixedCalculate.sqr(FixedCalculate.Max(0, agentTree_[agentTree_[node].right_].minY_ - position.Y)) + FixedCalculate.sqr(FixedCalculate.Max(0, position.Y - agentTree_[agentTree_[node].right_].maxY_));

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
    /// 
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
            FixedInt distSqLeft = FixedCalculate.sqr(FixedCalculate.Max(0, agentTree_[agentTree_[node].left_].minX_ - position.X)) + FixedCalculate.sqr(FixedCalculate.Max(0, position.X - agentTree_[agentTree_[node].left_].maxX_)) + FixedCalculate.sqr(FixedCalculate.Max(0, agentTree_[agentTree_[node].left_].minY_ - position.Y)) + FixedCalculate.sqr(FixedCalculate.Max(0, position.Y - agentTree_[agentTree_[node].left_].maxY_));
            FixedInt distSqRight = FixedCalculate.sqr(FixedCalculate.Max(0, agentTree_[agentTree_[node].right_].minX_ - position.X)) + FixedCalculate.sqr(FixedCalculate.Max(0, position.X - agentTree_[agentTree_[node].right_].maxX_)) + FixedCalculate.sqr(FixedCalculate.Max(0, agentTree_[agentTree_[node].right_].minY_ - position.Y)) + FixedCalculate.sqr(FixedCalculate.Max(0, position.Y - agentTree_[agentTree_[node].right_].maxY_));

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





    private   void GetRangeObstacleVertices(Agent agent, ObstacleVerticeTreeNode node, FixedInt rangeSq, NativeList<ObstacleVerticeNeighbor> obstacleNeighbors)
    {
        if (node.obstacleVertice_Index == -1) return;
        ObstacleVertice obstacle1 = obstacleVertices_[node.obstacleVertice_Index];
        ObstacleVertice obstacle2 = obstacleVertices_[obstacle1.next_];

        FixedInt agentLeftOfLine = FixedCalculate.leftOf(obstacle1.point_, obstacle2.point_, agent.position_);

        if (agentLeftOfLine >= 0)
        {
            if (node.left_index != -1) GetRangeObstacleVertices(agent, obstacleVerticesTree_[node.left_index], rangeSq, obstacleNeighbors);
        }
        else
        {
            if (node.right_index != -1) GetRangeObstacleVertices(agent, obstacleVerticesTree_[node.right_index], rangeSq, obstacleNeighbors);
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

        FixedInt distSq = FixedCalculate.distSqPointLineSegment(obstacle.point_, nextObstacle.point_, agent.position_);

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
