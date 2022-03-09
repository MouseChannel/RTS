using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

using Unity.Collections;
using FixedMath;
using System;



[DisableAutoCreation]
public partial class KDTreeSystem : WorkSystem
{

    private const int MAX_LEAF_SIZE = 10;
     
    public NativeList<Agent> agents_ = new NativeList<Agent>(Allocator.Persistent);
    public NativeList<AgentTreeNode> agentTree_ = new NativeList<AgentTreeNode>(Allocator.Persistent);
    public NativeList<ObstacleVertice> obstacleVertices_ = new NativeList<ObstacleVertice>(Allocator.Persistent);
    public NativeList<ObstacleVerticeTreeNode> obstacleVerticesTree_ = new NativeList<ObstacleVerticeTreeNode>(Allocator.Persistent);

    public ObstacleVerticeTreeNode obstacleVerticesTreeRoot;

    public NativeList<Obstacle> obstacles_ = new NativeList<Obstacle>(Allocator.Persistent);

    public NativeList<ObstacleTreeNode> obstacleTree_ = new NativeList<ObstacleTreeNode>(Allocator.Persistent);




    protected override void OnDestroy()
    {
        agents_.Dispose();
        agentTree_.Dispose();
        obstacleVertices_.Dispose();
        obstacleVerticesTree_.Dispose();

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

  




}


