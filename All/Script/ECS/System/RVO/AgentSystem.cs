using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Transforms;
using System;
using UnityEngine.UI;

namespace RVO
{


    public partial class AgentSystem : WorkSystem
    {
        public int aas = 3;


        public override void Work()
        {

            // Debug.Log(string.Format("{0}  ", DateTime.Now));
            /// <summary>
            /// Update three state
            /// 1：update agent position
            /// 2: (optional) update rangeNeighbor(heal stuff)
            /// 3: (optional) update enemyUnit(auto attack)
            /// </summary>
            /// <typeparam name="UpdateAgentJob"></typeparam>
            /// <returns></returns>

            #region  updateAgentJob
            var kDTreeSystem = World.GetExistingSystem<KDTreeSystem>();
            var agents_ = kDTreeSystem.agents_;
            var agentTree_ = kDTreeSystem.agentTree_;
            var obstacles_ = kDTreeSystem.obstacles_;
            var obstacleTree_ = kDTreeSystem.obstacleTree_;
            var obstacleTreeRoot = kDTreeSystem.obstacleTreeRoot;

            // Debug.Log(string.Format("{0}  ", DateTime.Now));



            var ecbPara = endSimulationEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
            NativeList<JobHandle> jobHandleList = new NativeList<JobHandle>(Allocator.Temp);


            Entities.ForEach((Entity entity, int entityInQueryIndex, in Agent agent) =>
            {
                UpdateAgentJob updateAgentJob = new UpdateAgentJob
                {
                    // newVelocity = newVelocity,
                    // rangeNeighbors = rangeNeighbors,
                    // enemyUnit = enemyUnit,
                    entity = entity,
                    agent = agent,
                    agents = agents_,
                    agentTree = agentTree_,
                    obstacles = obstacles_,
                    obstacleTree = obstacleTree_,
                    obstacleTreeRoot = obstacleTreeRoot,
                    indexInEntityQuery = entityInQueryIndex,
                    ecbPara = ecbPara

                };




                jobHandleList.Add(updateAgentJob.Schedule());

            }).WithoutBurst().Run();
            JobHandle.CompleteAll(jobHandleList);

            #endregion

            jobHandleList.Dispose();
  


        }




    }
}