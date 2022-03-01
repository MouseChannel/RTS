using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Transforms;
using System;

namespace RVO
{


    public partial class AgentSystem : WorkSystem
    {


        
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
            List<UpdateAgentJob> updateAgentJobList = new List<UpdateAgentJob>();

            Entities.ForEach((Entity entity, int entityInQueryIndex, in Agent agent) =>
            {
                // NativeArray<FixedVector2> newVelocity = new NativeArray<FixedVector2>(1, Allocator.TempJob);
                // NativeList<int> rangeNeighbors = new NativeList<int>(Allocator.TempJob);
                // NativeArray<int> enemyUnit = new NativeArray<int>(1, Allocator.TempJob);
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


                // updateAgentJobList.Add(updateAgentJob);

                jobHandleList.Add(updateAgentJob.Schedule());

            }).WithoutBurst().Run();
            JobHandle.CompleteAll(jobHandleList);

            #endregion



            foreach (var computeJob in updateAgentJobList)
            {

                // var newAgent = computeJob.agent;
                // newAgent.velocity_ = computeJob.newVelocity[0];
                // newAgent.position_ += newAgent.velocity_ / 5;
                // // newAgent.closestEnemy_ = computeJob.enemyUnit[0];


                // ecb.SetComponent<Agent>(computeJob.entity, newAgent);
                // var buf = ecb.AddBuffer<HealObject>(computeJob.entity);
                // foreach (var i in computeJob.rangeNeighbors)
                // {
                //     if (i != computeJob.agent.id_)
                //         buf.Add(new HealObject { healObjectNo = i });
                // }
                // computeJob.newVelocity.Dispose();
                // computeJob.enemyUnit.Dispose();
                // computeJob.rangeNeighbors.Dispose();


                // ecb.SetComponent<FOWUnit>(computeJob.entity, new FOWUnit{gridIndex =  GridSystem.Instance.GetGridIndex(newAgent.position_),range = 4});
            }

            jobHandleList.Dispose();



        }





    }
}