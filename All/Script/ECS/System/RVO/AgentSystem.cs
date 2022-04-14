// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using Unity.Entities;
// using Unity.Collections;
// using Unity.Jobs;
// using Unity.Transforms;
// using System;
// using UnityEngine.UI;
// using UnityEngine.Profiling;
// // [UpdateInGroup(typeof(CommandGroup))]
// // [UpdateAfter(typeof(KDTreeSystem))]
// public partial class AgentSystem : WorkSystem
// {
//     private KDTreeSystem kDTreeSystem;
//     private NativeList<Agent> agents_;
//     private NativeList<AgentTreeNode> agentTree_;
//     private NativeList<ObstacleVertice> obstacles_;
//     private NativeList<ObstacleVerticeTreeNode> obstacleTree_;

//     private ObstacleVerticeTreeNode obstacleTreeRoot;
//     private NativeList<Entity> entities;



//     protected override void OnStartRunning()
//     {
//         kDTreeSystem = GetSystem<KDTreeSystem>();
//     }





//     public override void Work()
//     {
//         // if (!ShouldRunSystem()) return;

//         #region  updateAgentJob
//         Profiler.BeginSample("AgentStart");
//         entities = GetSystem<ResponseNetSystem>().allMovedUnit;
// if(kDTreeSystem == null) return;
//         agents_ = kDTreeSystem.agents_;
        
//         agentTree_ = kDTreeSystem.agentTree_;
//         obstacles_ = kDTreeSystem.obstacleVertices_;
//         obstacleTree_ = kDTreeSystem.obstacleVerticesTree_;
//         obstacleTreeRoot = kDTreeSystem.obstacleVerticesTreeRoot;

//         // Debug.Log(string.Format("{0}  ", DateTime.Now));
//         Profiler.EndSample();


//         var ecbPara = endSimulationEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
//         NativeList<JobHandle> jobHandleList = new NativeList<JobHandle>(Allocator.Temp);


//         Entities.ForEach((Entity entity, int entityInQueryIndex, in Agent agent) =>
//         {

//             UpdateAgentJob updateAgentJob = new UpdateAgentJob
//             {
//                 // newVelocity = newVelocity,
//                 // rangeNeighbors = rangeNeighbors,
//                 // enemyUnit = enemyUnit,
//                 entity = entity,
//                 agent = agent,
//                 agents = agents_,
//                 agentTree = agentTree_,
//                 obstacles = obstacles_,
//                 obstacleTree = obstacleTree_,
//                 obstacleTreeRoot = obstacleTreeRoot,
//                 indexInEntityQuery = entityInQueryIndex,
//                 ecbPara = ecbPara

//             };




//             jobHandleList.Add(updateAgentJob.Schedule());

//         }).WithoutBurst().Run();
       

//         #endregion

//         jobHandleList.Dispose();

//         new UpdateAgentJobParallel
//         {
//             // newVelocity = newVelocity,
//             // rangeNeighbors = rangeNeighbors,
//             // enemyUnit = enemyUnit,
//             // entity = entity,
//             entities = entities,

//             agents = agents_,
//             agentTree = agentTree_,
//             obstacleVertices_ = obstacles_,
//             obstacleVerticesTree_ = obstacleTree_,
//             obstacleVerticesTreeRoot = obstacleTreeRoot,
//             // indexInEntityQuery = entityInQueryIndex,
//             ecbPara = ecbPara

//         }.Schedule(agents_.Length,4).Complete();

//  JobHandle.CompleteAll(jobHandleList);

//     }




// }
