using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

using Unity.Collections;
using FixedMath;
using System;

namespace RVO
{
    [DisableAutoCreation]
    public partial class KDTreeSystem : WorkSystem
    {
        int aaa = 3;
        private const int MAX_LEAF_SIZE = 10;
        private int ObstacleCount;
        public NativeList<Agent> agents_ = new NativeList<Agent>(Allocator.Persistent);
        public NativeList<AgentTreeNode> agentTree_ = new NativeList<AgentTreeNode>(Allocator.Persistent);
        public NativeList<Obstacle> obstacles_ = new NativeList<Obstacle>(Allocator.Persistent);
        public NativeList<ObstacleTreeNode> obstacleTree_ = new NativeList<ObstacleTreeNode>(Allocator.Persistent);

        public ObstacleTreeNode obstacleTreeRoot;
        protected override void OnDestroy()
        {
            agents_.Dispose();
            agentTree_.Dispose();
            obstacles_.Dispose();
            obstacleTree_.Dispose();


        }
        public override void Work()
        {
            #region Build Agent Tree

            agents_.Clear();
            agentTree_.Clear();



            Entities.ForEach((Entity entity, in Agent agent) =>
            {
                agents_.Add(agent);
                agentTree_.Add(new AgentTreeNode { });
                agentTree_.Add(new AgentTreeNode { });

            }).WithoutBurst().Run();



            BuildAgentTree(0, agents_.Length, 0);
            #endregion

            #region  Build Obstacle Tree

            obstacles_.Clear();
            obstacleTree_.Clear();
            // NativeList<Obstacle> obstacles_ = new NativeList<Obstacle>(Allocator.TempJob);
            // NativeList<ObstacleTreeNode> obstacleTree_ = new NativeList<ObstacleTreeNode>(Allocator.TempJob);
            Entities.ForEach((Entity entity, DynamicBuffer<ObstacleVertice> obstacleVertices) =>
            {
                ObstacleCollect(obstacles_, obstacleVertices);
            }).WithoutBurst().Run();

            InitObstacleTree(obstacleTree_, obstacles_.Length);
            obstacleTreeRoot = BuildObstacleTree(obstacles_, obstacles_, obstacleTree_);


            #endregion


        }

        private void BuildAgentTree(int begin, int end, int node)
        {
            if (agents_.Length == 0) return;
            var treeNode = agentTree_[node];
            treeNode.begin_ = begin;
            treeNode.end_ = end;
            treeNode.minX_ = treeNode.maxX_ = agents_[begin].position_.x_;
            treeNode.minY_ = treeNode.maxY_ = agents_[begin].position_.y_;
            agentTree_[node] = treeNode;
            for (int i = begin + 1; i < end; ++i)
            {
                treeNode.maxX_ = FixedCalculate.Max(agentTree_[node].maxX_, agents_[i].position_.x_);
                treeNode.minX_ = FixedCalculate.Min(agentTree_[node].minX_, agents_[i].position_.x_);
                treeNode.maxY_ = FixedCalculate.Max(agentTree_[node].maxY_, agents_[i].position_.y_);
                treeNode.minY_ = FixedCalculate.Min(agentTree_[node].minY_, agents_[i].position_.y_);

                agentTree_[node] = treeNode;
            }
            if (end - begin > MAX_LEAF_SIZE)
            {
                /* No leaf node. */
                bool isVertical = agentTree_[node].maxX_ - agentTree_[node].minX_ > agentTree_[node].maxY_ - agentTree_[node].minY_;
                FixedInt splitValue = FixedInt.half * (isVertical ? agentTree_[node].maxX_ + agentTree_[node].minX_ : agentTree_[node].maxY_ + agentTree_[node].minY_);

                int left = begin;
                int right = end;

                while (left < right)
                {
                    while (left < right && (isVertical ? agents_[left].position_.x_ : agents_[left].position_.y_) < splitValue)
                    {
                        ++left;
                    }

                    while (right > left && (isVertical ? agents_[right - 1].position_.x_ : agents_[right - 1].position_.y_) >= splitValue)
                    {
                        --right;
                    }

                    if (left < right)
                    {
                        Agent tempAgent = agents_[left];
                        agents_[left] = agents_[right - 1];
                        agents_[right - 1] = tempAgent;
                        ++left;
                        --right;
                    }
                }
                int leftSize = left - begin;

                if (leftSize == 0)
                {
                    ++leftSize;
                    ++left;
                    ++right;
                }
                treeNode.left_ = node + 1;
                treeNode.right_ = node + 2 * leftSize;
                agentTree_[node] = treeNode;

                // agentTree_[node].left_ = node + 1;
                // agentTree_[node].right_ = node + 2 * leftSize;

                // BuildAgentTree(agents_, agentTree_, begin, left, agentTree_[node].left_);
                // BuildAgentTree(agents_, agentTree_, left, end, agentTree_[node].right_);
                BuildAgentTree(begin, left, agentTree_[node].left_);
                BuildAgentTree(left, end, agentTree_[node].right_);
            }




        }

        private void ObstacleCollect(NativeList<Obstacle> obstacles_, DynamicBuffer<ObstacleVertice> obstacleVertices)
        {
            if (obstacleVertices.Length < 2)
            {
                return;
            }

            int obstacleNo = obstacles_.Length;
            for (int i = 0; i < obstacleVertices.Length; ++i)
            {
                obstacles_.Add(new Obstacle
                {
                    id_ = obstacles_.Length,
                    previous_ = -1,
                    next_ = -1
                });
            }

            for (int i = 0; i < obstacleVertices.Length; ++i)
            {

                Obstacle obstacle = obstacles_[obstacles_.Length - obstacleVertices.Length + i];
                obstacle.point_ = obstacleVertices[i].vertice;

                if (i != 0)
                {

                    obstacle.previous_ = obstacle.id_ - 1;

                    var temp = obstacles_[obstacle.previous_];
                    temp.next_ = obstacle.id_;
                    obstacles_[obstacle.previous_] = temp;

                }

                if (i == obstacleVertices.Length - 1)
                {
                    obstacle.next_ = obstacles_[obstacleNo].id_;

                    var temp = obstacles_[obstacle.next_];
                    temp.previous_ = obstacle.id_;
                    obstacles_[obstacle.next_] = temp;
                    // obstacle.next_.previous_ = obstacle;
                }

                obstacle.direction_ = RVOMath.normalize(obstacleVertices[(i == obstacleVertices.Length - 1 ? 0 : i + 1)].vertice - obstacleVertices[i].vertice);

                if (obstacleVertices.Length == 2)
                {
                    obstacle.convex_ = true;
                }
                else
                {
                    obstacle.convex_ = (RVOMath.leftOf(obstacleVertices[(i == 0 ? obstacleVertices.Length - 1 : i - 1)].vertice, obstacleVertices[i].vertice, obstacleVertices[(i == obstacleVertices.Length - 1 ? 0 : i + 1)].vertice) >= 0);
                }
                obstacles_[obstacle.id_] = obstacle;


            }


        }

        private void InitObstacleTree(NativeList<ObstacleTreeNode> obstacleTree, int length)
        {
            for (int i = 0; i < length; i++)
            {
                obstacleTree.Add(new ObstacleTreeNode
                {
                    obstacleIndex = i,
                    left_index = -1,
                    right_index = -1
                });
            }
        }


        private ObstacleTreeNode BuildObstacleTree(NativeList<Obstacle> current, NativeList<Obstacle> obstacles, NativeList<ObstacleTreeNode> obstacleTree_)
        {
            if (current.Length == 0) return new ObstacleTreeNode { obstacleIndex = -1 };
            ObstacleCount = 0;
            ObstacleTreeNode node = new ObstacleTreeNode();
            int length = current.Length;

            int optimalSplit = 0;
            int minLeft = current.Length; ;
            int minRight = current.Length;

            for (int i = 0; i < current.Length; ++i)
            {
                int leftSize = 0;
                int rightSize = 0;

                Obstacle obstacleI1 = current[i];
                Obstacle obstacleI2 = obstacles[obstacleI1.next_];

                /* Compute optimal split node. */
                for (int j = 0; j < current.Length; ++j)
                {
                    if (i == j)
                    {
                        continue;
                    }

                    Obstacle obstacleJ1 = current[j];



                    Obstacle obstacleJ2 = obstacles[obstacleJ1.next_];

                    FixedInt j1LeftOfI = RVOMath.leftOf(obstacleI1.point_, obstacleI2.point_, obstacleJ1.point_);
                    FixedInt j2LeftOfI = RVOMath.leftOf(obstacleI1.point_, obstacleI2.point_, obstacleJ2.point_);


                    if (j1LeftOfI >= -FixedCalculate.superSmallValue && j2LeftOfI >= -FixedCalculate.superSmallValue)
                    {
                        ++leftSize;
                    }
                    else if (j1LeftOfI <= FixedCalculate.superSmallValue && j2LeftOfI <= FixedCalculate.superSmallValue)
                    {
                        ++rightSize;
                    }
                    else
                    {
                        ++leftSize;
                        ++rightSize;
                    }

                    if (new FixedIntPair(FixedCalculate.Max(leftSize, rightSize), FixedCalculate.Min(leftSize, rightSize)) >= new FixedIntPair(FixedCalculate.Max(minLeft, minRight), FixedCalculate.Min(minLeft, minRight)))
                    {
                        break;
                    }
                }


                if (new FixedIntPair(FixedCalculate.Max(leftSize, rightSize), FixedCalculate.Min(leftSize, rightSize)) < new FixedIntPair(FixedCalculate.Max(minLeft, minRight), FixedCalculate.Min(minLeft, minRight)))
                {
                    minLeft = leftSize;
                    minRight = rightSize;

                    optimalSplit = i;
                }
            }

            {
                /* Build split node. */
                NativeList<Obstacle> leftObstacles = new NativeList<Obstacle>(Allocator.Temp);
                // IList<Obstacle> leftObstacles = new List<Obstacle>(minLeft);

                for (int n = 0; n < minLeft; ++n)
                {
                    leftObstacles.Add(new Obstacle { id_ = -1 });
                }

                NativeList<Obstacle> rightObstacles = new NativeList<Obstacle>(Allocator.Temp);
                // IList<Obstacle> rightObstacles = new List<Obstacle>(minRight);

                for (int n = 0; n < minRight; ++n)
                {
                    rightObstacles.Add(new Obstacle { id_ = -1 });
                }

                int leftCounter = 0;
                int rightCounter = 0;
                int i = optimalSplit;

                Obstacle obstacleI1 = current[i];
                Obstacle obstacleI2 = obstacles[obstacleI1.next_];

                for (int j = 0; j < current.Length; ++j)
                {
                    if (i == j)
                    {
                        continue;
                    }

                    Obstacle obstacleJ1 = current[j];
                    Obstacle obstacleJ2 = obstacles[obstacleJ1.next_];

                    FixedInt j1LeftOfI = RVOMath.leftOf(obstacleI1.point_, obstacleI2.point_, obstacleJ1.point_);
                    FixedInt j2LeftOfI = RVOMath.leftOf(obstacleI1.point_, obstacleI2.point_, obstacleJ2.point_);


                    if (j1LeftOfI >= -FixedCalculate.superSmallValue && j2LeftOfI >= -FixedCalculate.superSmallValue)
                    {
                        leftObstacles[leftCounter++] = current[j];
                    }
                    else if (j1LeftOfI <= FixedCalculate.superSmallValue && j2LeftOfI <= FixedCalculate.superSmallValue)
                    {
                        rightObstacles[rightCounter++] = current[j];
                    }
                    else
                    {
                        /* Split obstacle j. */
                        FixedInt t = RVOMath.det(obstacleI2.point_ - obstacleI1.point_, obstacleJ1.point_ - obstacleI1.point_) / RVOMath.det(obstacleI2.point_ - obstacleI1.point_, obstacleJ1.point_ - obstacleJ2.point_);

                        Vector2 splitPoint = obstacleJ1.point_ + t * (obstacleJ2.point_ - obstacleJ1.point_);

                        Obstacle newObstacle = new Obstacle();
                        newObstacle.point_ = splitPoint;
                        newObstacle.previous_ = obstacleJ1.id_;
                        newObstacle.next_ = obstacleJ2.id_;
                        newObstacle.convex_ = true;
                        newObstacle.direction_ = obstacleJ1.direction_;

                        newObstacle.id_ = ObstacleCount;
                        ObstacleCount++;

                        // Simulator.Instance.obstacles_.Add(newObstacle);
                        obstacles.Add(newObstacle);

                        obstacleJ1.next_ = newObstacle.id_;
                        obstacleJ2.previous_ = newObstacle.id_;

                        if (j1LeftOfI > 0)
                        {
                            leftObstacles[leftCounter++] = obstacleJ1;
                            rightObstacles[rightCounter++] = newObstacle;
                        }
                        else
                        {
                            rightObstacles[rightCounter++] = obstacleJ1;
                            leftObstacles[leftCounter++] = newObstacle;
                        }

                    }


                }

                node.obstacleIndex = obstacleI1.id_;

                node.left_index = BuildObstacleTree(leftObstacles, obstacles, obstacleTree_).obstacleIndex;
                node.right_index = BuildObstacleTree(rightObstacles, obstacles, obstacleTree_).obstacleIndex;
                obstacleTree_[node.obstacleIndex] = node;
                leftObstacles.Dispose();
                rightObstacles.Dispose();

                return node;
            }
        }

        #region  selection
        public void GetClosestAgent(Vector2 position, ref FixedInt rangeSq, ref int agentNo, int node)
        {
 

            if (agentTree_[node].end_ - agentTree_[node].begin_ <= MAX_LEAF_SIZE)
            {
                for (int i = agentTree_[node].begin_; i < agentTree_[node].end_; ++i)
                {
                    Debug.Log(agents_[i].id_);
                    FixedInt distSq = RVOMath.absSq(position - agents_[i].position_);
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
                FixedInt distSqLeft = RVOMath.sqr(FixedCalculate.Max(0, agentTree_[agentTree_[node].left_].minX_ - position.x_)) + RVOMath.sqr(FixedCalculate.Max(0, position.x_ - agentTree_[agentTree_[node].left_].maxX_)) + RVOMath.sqr(FixedCalculate.Max(0, agentTree_[agentTree_[node].left_].minY_ - position.y_)) + RVOMath.sqr(FixedCalculate.Max(0, position.y_ - agentTree_[agentTree_[node].left_].maxY_));
                FixedInt distSqRight = RVOMath.sqr(FixedCalculate.Max(0, agentTree_[agentTree_[node].right_].minX_ - position.x_)) + RVOMath.sqr(FixedCalculate.Max(0, position.x_ - agentTree_[agentTree_[node].right_].maxX_)) + RVOMath.sqr(FixedCalculate.Max(0, agentTree_[agentTree_[node].right_].minY_ - position.y_)) + RVOMath.sqr(FixedCalculate.Max(0, position.y_ - agentTree_[agentTree_[node].right_].maxY_));
         
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
        public void GetAreaAgents(Vector2 position, Vector4 areaRect, FixedInt rangeSq, int node, List<int> areaAgents)
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
                FixedInt distSqLeft = RVOMath.sqr(FixedCalculate.Max(0, agentTree_[agentTree_[node].left_].minX_ - position.x_)) + RVOMath.sqr(FixedCalculate.Max(0, position.x_ - agentTree_[agentTree_[node].left_].maxX_)) + RVOMath.sqr(FixedCalculate.Max(0, agentTree_[agentTree_[node].left_].minY_ - position.y_)) + RVOMath.sqr(FixedCalculate.Max(0, position.y_ - agentTree_[agentTree_[node].left_].maxY_));
                FixedInt distSqRight = RVOMath.sqr(FixedCalculate.Max(0, agentTree_[agentTree_[node].right_].minX_ - position.x_)) + RVOMath.sqr(FixedCalculate.Max(0, position.x_ - agentTree_[agentTree_[node].right_].maxX_)) + RVOMath.sqr(FixedCalculate.Max(0, agentTree_[agentTree_[node].right_].minY_ - position.y_)) + RVOMath.sqr(FixedCalculate.Max(0, position.y_ - agentTree_[agentTree_[node].right_].maxY_));

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
        

            if (Vaild(pos.x_.RawFloat, areaRect[0], areaRect[1]) &&
                        Vaild(pos.y_.RawFloat, areaRect[2], areaRect[3]))
            {
                areaAgents.Add(agent.id_);
            }
        }
    }
    #endregion
}
