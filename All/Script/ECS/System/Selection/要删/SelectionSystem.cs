// using System;
// using System.Collections.Generic;
// using Unity.Entities;
// using UnityEngine;
// using Unity.Physics;
// using Unity.Physics.Systems;
// using UnityEngine.UI;

// using Unity.Mathematics;
// using Unity.Collections;
// using Unity.Transforms;
// using RaycastHit = Unity.Physics.RaycastHit;
// using Vector2 = UnityEngine.Vector2;


// [AlwaysUpdateSystem]
// [DisableAutoCreation]
// [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
// public class SelectionSystem : SystemBase
// {
//     private Camera mainCamera;
//     private BuildPhysicsWorld buildPhysicsWorld;
//     private CollisionWorld collisionWorld;
//     public bool IsDragging = false;
//     private RectTransform selectionBoxRect;
//     public Vector3 mouseStartPos;
    
//     public List<int> selectUnits = new List<int>();


//     protected override void OnCreate()
//     {

//         buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
     



//     }
//     public void GetMainCamera( ){
//         mainCamera = Camera.main;
//     }
//     protected override void OnUpdate()
//     {
//         if(mainCamera == null) return;

//         if (Input.GetMouseButtonDown(0))
//         {
//             mouseStartPos = Input.mousePosition;
//         }
//         if (Input.GetMouseButton(0) && !IsDragging)
//         {
//             IsDragging = Vector3.Distance(mouseStartPos, Input.mousePosition) > 25;
//         }
//         if(IsDragging){
//             DrawSelectBox();
//         }

//         if (Input.GetMouseButtonUp(0))
//         {
//             DeSelectAll();

//             if (IsDragging)
//             {
//                 SelectMultipleUnit();
//             }
//             else
//             {
//                 SelectSingleUnit();
//             }
//         }







//     }

//     private void DrawSelectBox()
//     {
//         if(selectionBoxRect == null){
//             ResourceService.Instance.LoadMainWindow("UI/UIMainWindow/SelectionVisualBox", ref selectionBoxRect);
//         }

//         var mouseEndPos = Input.mousePosition;
//         selectionBoxRect.position = (mouseStartPos + mouseEndPos) / 2;
//         selectionBoxRect.sizeDelta = new Vector2(Mathf.Abs(mouseStartPos.x - mouseEndPos.x), Mathf.Abs(mouseStartPos.y - mouseEndPos.y)) ;


//     }

//     private void SelectMultipleUnit()
//     {
//         IsDragging = false;

//         var mousePosNow = Input.mousePosition;
//         ModifyRect(mousePosNow, mouseStartPos,
//                                 out Vector2 topLeft,
//                                 out Vector2 topRight,
//                                 out Vector2 botLeft,
//                                 out Vector2 botRight
//                                 );





//         var rect = Rect.MinMaxRect(topLeft.x, topLeft.y, botRight.x, botRight.y);

//         var cornerRays = new[]
//         {

//             mainCamera.ScreenPointToRay(topLeft),
//             mainCamera.ScreenPointToRay(topRight),
//             mainCamera.ScreenPointToRay(botLeft),
//             mainCamera.ScreenPointToRay(botRight),

//         };

//         var vertices = new NativeArray<float3>(5, Allocator.Temp);

//         for (var i = 0; i < cornerRays.Length; i++)
//         {
//             vertices[i] = cornerRays[i].GetPoint(50f);
//         }

//         vertices[4] = mainCamera.transform.position;

//         var collisionFilter = new CollisionFilter
//         {
//             BelongsTo = (uint)PhysicsLayer.SelectionBox,
//             CollidesWith = (uint)PhysicsLayer.Unit
//         };

//         var physicsMaterial = Unity.Physics.Material.Default;
//         physicsMaterial.CollisionResponse = CollisionResponsePolicy.RaiseTriggerEvents;

//         var selectionCollider = ConvexCollider.Create(vertices, ConvexHullGenerationParameters.Default,
//             collisionFilter, physicsMaterial);

//         var selectionArchetype = EntityManager.CreateArchetype(typeof(PhysicsCollider), typeof(LocalToWorld),
//             typeof(SelectionColliderTag));

//         var newSelectionEntity = EntityManager.CreateEntity(selectionArchetype);

//         EntityManager.SetComponentData(newSelectionEntity, new PhysicsCollider { Value = selectionCollider });





//     }

//     private void ModifyRect(Vector3 mousePosNow, Vector3 mouseStartPos, out Vector2 topLeft, out Vector2 topRight, out Vector2 botLeft, out Vector2 botRight)
//     {
//         topLeft = new Vector2(math.min(mousePosNow.x, mouseStartPos.x), math.max(mousePosNow.y, mouseStartPos.y));
//         topRight = new Vector2(math.max(mousePosNow.x, mouseStartPos.x), math.max(mousePosNow.y, mouseStartPos.y));
//         botLeft = new Vector2(math.min(mousePosNow.x, mouseStartPos.x), math.min(mousePosNow.y, mouseStartPos.y));
//         botRight = new Vector2(math.max(mousePosNow.x, mouseStartPos.x), math.min(mousePosNow.y, mouseStartPos.y));

//     }

//     private void DeSelectAll()
//     {
//         selectUnits.Clear();
//         EntityManager.RemoveComponent<SelectableEntityTag>(GetEntityQuery(typeof(SelectableEntityTag)));
//         selectionBoxRect.sizeDelta = Vector2.zero;
//     }

//     private void SelectSingleUnit()
//     {

//         collisionWorld = buildPhysicsWorld.PhysicsWorld.CollisionWorld;
//         var ray = mainCamera.ScreenPointToRay(Input.mousePosition);

//         var rayStart = ray.origin;
//         var rayEnd = ray.GetPoint(50);

//         if (Raycast(rayStart, rayEnd, out var raycastHit))
//         {

          
//             var hitEntity = buildPhysicsWorld.PhysicsWorld.Bodies[raycastHit.RigidBodyIndex].Entity;
//             if (EntityManager.HasComponent<CanBeSelected>(hitEntity))
//             {
//                 EntityManager.AddComponent<SelectableEntityTag>(hitEntity);
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
//                 CollidesWith = (uint)PhysicsLayer.Unit
//             },
//         };
//         return collisionWorld.CastRay(raycastInput, out raycastHit);
//     }



// }
