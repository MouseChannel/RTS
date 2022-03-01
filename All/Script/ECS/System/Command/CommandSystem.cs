// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using Unity.Entities;
// using Unity.Mathematics;
// using Unity.Physics;
// using RaycastHit = Unity.Physics.RaycastHit;
// using Unity.Physics.Systems;
// using Pb;
// using RVO;
// [AlwaysUpdateSystem]
// //  [UpdateBefore(typeof(ResponseCommandSystem))]
//  [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
//  [DisableAutoCreation]
// public class CommandSystem : SystemBase
// {
//     private CollisionWorld collisionWorld;
//     private BuildPhysicsWorld buildPhysicsWorld;
//     private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBufferSystem;
//     protected override void OnCreate()
//     {
//         buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
//         endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();


//     }
//     protected override void OnUpdate()
//     {
//         if (Input.GetMouseButtonDown(1))
//         {
//             var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

//             var rayStart = ray.origin;
//             var rayEnd = ray.GetPoint(50f);
//             collisionWorld = buildPhysicsWorld.PhysicsWorld.CollisionWorld;
//             var selectionSystem = World.GetOrCreateSystem<SelectionSystem>();

//             if (Raycast(rayStart, rayEnd, out var raycastHit))
//             {
//                 var hitEntity = buildPhysicsWorld.PhysicsWorld.Bodies[raycastHit.RigidBodyIndex].Entity;
//                 var ecb = endSimulationEntityCommandBufferSystem.CreateCommandBuffer();

//                 selectionSystem.selectUnits.Clear();
//                 Entities.ForEach((Entity entity, in SelectableEntityTag selectableEntityTag, in Agent agent) =>
//                 {
//                     selectionSystem.selectUnits.Add(agent.id_);

//                 }).WithoutBurst().Run();

//                 if (EntityManager.HasComponent<GroungTag>(hitEntity))
//                 {

//                     if (selectionSystem.selectUnits.Count > 0)
//                     {
//                         // NetService.Instance.SendMessage(PbTool.MakeMove(raycastHit.Position, selectionSystem.selectUnits));
//                     }
//                 }
//                 else if (EntityManager.HasComponent<UnitTag>(hitEntity))
//                 {

//                 }
//                 else if (EntityManager.HasComponent<InteractableTag>(hitEntity))
//                 {

//                 }




//             }



//         }

//     }

//     private bool Raycast(float3 rayStart, float3 rayEnd, out RaycastHit raycastHit)
//     {

//         var raycastInput = new RaycastInput
//         {
//             Start = rayStart,
//             End = rayEnd,
//             Filter = new CollisionFilter
//             {
//                 BelongsTo = (uint)PhysicsLayer.SelectionBox,
//                 CollidesWith = (uint)PhysicsLayer.Unit |
//                                 (uint)PhysicsLayer.Building |
//                                 (uint)PhysicsLayer.Ground |
//                                 (uint)PhysicsLayer.Interactable
//             },
//         };
//         return collisionWorld.CastRay(raycastInput, out raycastHit);
//     }
// }
