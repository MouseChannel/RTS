using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Transforms;

namespace RVO
{

    public partial class AgentSystem : WorkSystem
    {

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        public override void Work()
        {
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




            NativeList<JobHandle> jobHandleList = new NativeList<JobHandle>(Allocator.Temp);
            List<UpdateAgentJob> updateAgentJobList = new List<UpdateAgentJob>();

            Entities.ForEach((Entity entity, int entityInQueryIndex, ref Agent agent) =>
            {
                NativeArray<Vector2> newVelocity = new NativeArray<Vector2>(1, Allocator.TempJob);
                // NativeList<int> rangeNeighbors = new NativeList<int>(Allocator.TempJob);
                // NativeArray<int> enemyUnit = new NativeArray<int>(1, Allocator.TempJob);
                UpdateAgentJob updateAgentJob = new UpdateAgentJob
                {
                    newVelocity = newVelocity,
                    // rangeNeighbors = rangeNeighbors,
                    // enemyUnit = enemyUnit,
                    entity = entity,
                    agent = agent,
                    agents = agents_,
                    agentTree = agentTree_,
                    obstacles = obstacles_,
                    obstacleTree = obstacleTree_,
                    obstacleTreeRoot = obstacleTreeRoot,

                };


                updateAgentJobList.Add(updateAgentJob);
                
                jobHandleList.Add(updateAgentJob.Schedule());

            }).WithoutBurst().Run();
            JobHandle.CompleteAll(jobHandleList);

            #endregion

            EntityCommandBuffer ecb = endFixedStepSimulationEntityCommandBufferSystem.CreateCommandBuffer();

            foreach (var computeJob in updateAgentJobList)
            {

                var newAgent = computeJob.agent;
                newAgent.velocity_ = computeJob.newVelocity[0];
                newAgent.position_ += newAgent.velocity_ / 5;
                // newAgent.closestEnemy_ = computeJob.enemyUnit[0];


                ecb.SetComponent<Agent>(computeJob.entity, newAgent);
                // var buf = ecb.AddBuffer<HealObject>(computeJob.entity);
                // foreach (var i in computeJob.rangeNeighbors)
                // {
                //     if (i != computeJob.agent.id_)
                //         buf.Add(new HealObject { healObjectNo = i });
                // }
                computeJob.newVelocity.Dispose();
                // computeJob.enemyUnit.Dispose();
                // computeJob.rangeNeighbors.Dispose();
                ecb.SetComponent<FOWUnit>(computeJob.entity, new FOWUnit{gridIndex =  GridInit.Instance.pathfindingGrid.GetGridIndex(newAgent.position_),range = 4});
            }

            jobHandleList.Dispose();



        }





    }
}