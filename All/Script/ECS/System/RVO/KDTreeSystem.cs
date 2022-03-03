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

        private const int MAX_LEAF_SIZE = 10;
        private int obstacleCount;
        public NativeList<Agent> agents_ = new NativeList<Agent>(Allocator.Persistent);
        public NativeList<AgentTreeNode> agentTree_ = new NativeList<AgentTreeNode>(Allocator.Persistent);
        public NativeList<Obstacle> obstacles_ = new NativeList<Obstacle>(Allocator.Persistent);
        public NativeList<ObstacleTreeNode> obstacleTree_ = new NativeList<ObstacleTreeNode>(Allocator.Persistent);

        private NativeList<Obstacle> splitObstaclesDuringBuild = new NativeList<Obstacle>(Allocator.Persistent);
        public ObstacleTreeNode obstacleTreeRoot;
        protected override void OnDestroy()
        {
            agents_.Dispose();
            agentTree_.Dispose();
            obstacles_.Dispose();
            obstacleTree_.Dispose();
            splitObstaclesDuringBuild.Dispose();


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


        }
 

        public void UpdateObstacleTree()
        {
            obstacles_.Clear();
            obstacleTree_.Clear();

            Entities.ForEach((Entity entity, DynamicBuffer<ObstacleVertice> obstacleVertices) =>
            {
                ObstacleCollect(obstacles_, obstacleVertices);
            }).WithoutBurst().Run();

 

            InitObstacleTree(obstacles_.Length);
            NativeList<Obstacle> currentObstacles = new NativeList<Obstacle> (  Allocator.Temp);
            currentObstacles.AddRange(obstacles_);
            obstacleTreeRoot = BuildObstacleTreeRecursive(currentObstacles);

         
            currentObstacles.Dispose();

 
        }

        private void BuildAgentTree(int begin, int end, int node)
        {
            if (agents_.Length == 0) return;
            var treeNode = agentTree_[node];
            treeNode.begin_ = begin;
            treeNode.end_ = end;
            treeNode.minX_ = treeNode.maxX_ = agents_[begin].position_.X;
            treeNode.minY_ = treeNode.maxY_ = agents_[begin].position_.Y;
            agentTree_[node] = treeNode;
            for (int i = begin + 1; i < end; ++i)
            {
                treeNode.maxX_ = FixedCalculate.Max(agentTree_[node].maxX_, agents_[i].position_.X);
                treeNode.minX_ = FixedCalculate.Min(agentTree_[node].minX_, agents_[i].position_.X);
                treeNode.maxY_ = FixedCalculate.Max(agentTree_[node].maxY_, agents_[i].position_.Y);
                treeNode.minY_ = FixedCalculate.Min(agentTree_[node].minY_, agents_[i].position_.Y);

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
                    while (left < right && (isVertical ? agents_[left].position_.X : agents_[left].position_.Y) < splitValue)
                    {
                        ++left;
                    }

                    while (right > left && (isVertical ? agents_[right - 1].position_.X : agents_[right - 1].position_.Y) >= splitValue)
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

                obstacle.direction_ = FixedCalculate.normalize(obstacleVertices[(i == obstacleVertices.Length - 1 ? 0 : i + 1)].vertice - obstacleVertices[i].vertice);

                if (obstacleVertices.Length == 2)
                {
                    obstacle.convex_ = true;
                }
                else
                {
                    obstacle.convex_ = (FixedCalculate.leftOf(obstacleVertices[(i == 0 ? obstacleVertices.Length - 1 : i - 1)].vertice, obstacleVertices[i].vertice, obstacleVertices[(i == obstacleVertices.Length - 1 ? 0 : i + 1)].vertice) >= 0);
                }
                obstacles_[obstacle.id_] = obstacle;


            }


        }

        private void InitObstacleTree(int length)
        {
            for (int i = 0; i < length; i++)
            {
                obstacleTree_.Add(new ObstacleTreeNode
                {
                    obstacleIndex = i,
                    left_index = -1,
                    right_index = -1
                });
            }
        }

 
    private ObstacleTreeNode BuildObstacleTreeRecursive(NativeList<Obstacle> current)
        {
            if (current.Length == 0) return new ObstacleTreeNode { obstacleIndex = -1 };
       
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
                Obstacle obstacleI2 = obstacles_[obstacleI1.next_];

                /* Compute optimal split node. */
                for (int j = 0; j < current.Length; ++j)
                {
                    if (i == j)
                    {
                        continue;
                    }

                    Obstacle obstacleJ1 = current[j];



                    Obstacle obstacleJ2 = obstacles_[obstacleJ1.next_];

                    FixedInt j1LeftOfI = FixedCalculate.leftOf(obstacleI1.point_, obstacleI2.point_, obstacleJ1.point_);
                    FixedInt j2LeftOfI = FixedCalculate.leftOf(obstacleI1.point_, obstacleI2.point_, obstacleJ2.point_);


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
                Obstacle obstacleI2 = obstacles_[obstacleI1.next_];

                for (int j = 0; j < current.Length; ++j)
                {
                    if (i == j)
                    {
                        continue;
                    }

                    Obstacle obstacleJ1 = current[j];
                    Obstacle obstacleJ2 = obstacles_[obstacleJ1.next_];

                    FixedInt j1LeftOfI = FixedCalculate.leftOf(obstacleI1.point_, obstacleI2.point_, obstacleJ1.point_);
                    FixedInt j2LeftOfI = FixedCalculate.leftOf(obstacleI1.point_, obstacleI2.point_, obstacleJ2.point_);


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
                        FixedInt t = FixedCalculate.det(obstacleI2.point_ - obstacleI1.point_, obstacleJ1.point_ - obstacleI1.point_) / FixedCalculate.det(obstacleI2.point_ - obstacleI1.point_, obstacleJ1.point_ - obstacleJ2.point_);

                        FixedVector2 splitPoint = obstacleJ1.point_ + t * (obstacleJ2.point_ - obstacleJ1.point_);

                        Obstacle newObstacle = new Obstacle();
                        newObstacle.point_ = splitPoint;
                        newObstacle.previous_ = obstacleJ1.id_;
                        newObstacle.next_ = obstacleJ2.id_;
                        newObstacle.convex_ = true;
                        newObstacle.direction_ = obstacleJ1.direction_;

                        newObstacle.id_ = obstacles_.Length;
                       

                        // Simulator.Instance.obstacles_.Add(newObstacle);
                        obstacles_.Add(newObstacle);
                        obstacleTree_.Add(new ObstacleTreeNode());

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

                node.left_index = BuildObstacleTreeRecursive(leftObstacles ).obstacleIndex;
                node.right_index = BuildObstacleTreeRecursive(rightObstacles ).obstacleIndex;
                obstacleTree_[node.obstacleIndex] = node;
                leftObstacles.Dispose();
                rightObstacles.Dispose();

                return node;
            }
        }







    }

}
