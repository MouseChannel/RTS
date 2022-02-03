using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine.UI;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Transforms;
using RaycastHit = Unity.Physics.RaycastHit;
using Vector2 = UnityEngine.Vector2;
 
 
[AlwaysUpdateSystem]
public class SelectionSystem : SystemBase
{
    private Camera mainCamera;
    private BuildPhysicsWorld buildPhysicsWorld;
    private CollisionWorld collisionWorld;
    public bool IsDragging = false;
    public float3 mouseStartPos;
    private BeginPresentationEntityCommandBufferSystem beginInitializationEntityCommandBufferSystem;
 
    protected override void OnCreate()
    {
        mainCamera  = Camera.main;
        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        beginInitializationEntityCommandBufferSystem = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem>();
        
        
         
    }
    protected override void OnUpdate()
    {
        if(Input.GetMouseButtonDown(0)){
            mouseStartPos = Input.mousePosition;
        }
        if(Input.GetMouseButton(0) && !IsDragging){
            IsDragging = math.distance(mouseStartPos, Input.mousePosition) > 25;
        }
  
        if(Input.GetMouseButtonUp(0)){
            DeSelectAll();
            if(IsDragging){
                SelectMultipleUnit();
            }
            else{
                SelectSingleUnit();
            }
        }


        



        
    }

    private void SelectMultipleUnit()
    {
        IsDragging = false;
         
        var mousePosNow = Input.mousePosition;
        ModifyRect(mousePosNow, mouseStartPos,
                                out Vector2 topLeft,
                                out Vector2 topRight,
                                out Vector2 botLeft,
                                out Vector2 botRight
                                );
        
       
       
         

        var rect = Rect.MinMaxRect(topLeft.x, topLeft.y, botRight.x, botRight.y);

        var cornerRays = new[]
        {
            
            Camera.main.ScreenPointToRay(topLeft),
            Camera.main.ScreenPointToRay(topRight),
            Camera.main.ScreenPointToRay(botLeft),
            Camera.main.ScreenPointToRay(botRight),
 
        };

        var vertices = new NativeArray<float3>(5, Allocator.Temp);

        for (var i = 0; i < cornerRays.Length; i++)
        {
            vertices[i] = cornerRays[i].GetPoint(50f);
        }

        vertices[4] = Camera.main.transform.position;
        
        var collisionFilter = new CollisionFilter
        {
            BelongsTo = (uint) 1<<2,
            CollidesWith = (uint) 1<<0
        };
        
        var physicsMaterial = Unity.Physics.Material.Default;
        physicsMaterial.CollisionResponse = CollisionResponsePolicy.RaiseTriggerEvents;

        var selectionCollider = ConvexCollider.Create(vertices, ConvexHullGenerationParameters.Default,
            collisionFilter, physicsMaterial);

        var selectionArchetype = EntityManager.CreateArchetype(typeof(PhysicsCollider), typeof(LocalToWorld),
            typeof(SelectionColliderTag));

        var newSelectionEntity = EntityManager.CreateEntity(selectionArchetype);
    
        EntityManager.SetComponentData(newSelectionEntity, new PhysicsCollider{Value = selectionCollider});


        var ecb = beginInitializationEntityCommandBufferSystem.CreateCommandBuffer();
    
       
    }

    private void ModifyRect(Vector3 mousePosNow, Vector3 mouseStartPos, out Vector2 topLeft, out Vector2 topRight, out Vector2 botLeft, out Vector2 botRight){
        topLeft = new  UnityEngine.Vector2(math.min(mousePosNow.x, mouseStartPos.x), math.max(mousePosNow.y, mouseStartPos.y));
        topRight = new  UnityEngine.Vector2(math.max(mousePosNow.x, mouseStartPos.x), math.max(mousePosNow.y, mouseStartPos.y));
        botLeft = new  UnityEngine.Vector2(math.min(mousePosNow.x, mouseStartPos.x), math.min(mousePosNow.y, mouseStartPos.y));
        botRight = new  UnityEngine.Vector2(math.max(mousePosNow.x, mouseStartPos.x), math.max(mousePosNow.y, mouseStartPos.y));


    }
 
    private void DeSelectAll(){
        
        EntityManager.RemoveComponent<SelectableEntityTag>(GetEntityQuery(typeof(SelectableEntityTag)));
    }

    private void SelectSingleUnit()
    {
        
        collisionWorld = buildPhysicsWorld.PhysicsWorld.CollisionWorld;
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    
        var rayStart = ray.origin;
        var rayEnd = ray.GetPoint(1000);
       
        if(Raycast(rayStart, rayEnd, out var raycastHit)){
      
            var hitEntity = buildPhysicsWorld.PhysicsWorld.Bodies[raycastHit.RigidBodyIndex].Entity;
            if(EntityManager.HasComponent<CanBeSelected>(hitEntity)){
                EntityManager.AddComponent<SelectableEntityTag>(hitEntity);
            }
        }    
    }

    private bool Raycast(float3 rayStart, float3 rayEnd, out RaycastHit raycastHit){
        
        var raycastInput = new RaycastInput{
            Start = rayStart,
            End = rayEnd,
            Filter = new CollisionFilter{
                BelongsTo = (uint)1<<2,
                CollidesWith = (uint)1<<0,
            },
        };
        return collisionWorld.CastRay(raycastInput , out raycastHit);
    }


    
}
