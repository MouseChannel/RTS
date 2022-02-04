using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using RaycastHit = Unity.Physics.RaycastHit;
using Unity.Physics.Systems;

public class CommandSystem : SystemBase
{
    private CollisionWorld collisionWorld;
    private BuildPhysicsWorld buildPhysicsWorld;
    protected override void OnCreate()
    {
        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
    }
    protected override void OnUpdate()
    {
        if(Input.GetMouseButtonDown(1)){

        }
    }

    private bool Raycast(float3 rayStart, float3 rayEnd, out RaycastHit raycastHit){
        
        var raycastInput = new RaycastInput{
            Start = rayStart,
            End = rayEnd,
            Filter = new CollisionFilter{
                BelongsTo = (uint)PhysicsLayer.SelectionBox,
                CollidesWith = (uint)PhysicsLayer.Unit|
                                (uint)PhysicsLayer.Building|
                                (uint)PhysicsLayer.Ground|
                                (uint)PhysicsLayer.Interactable
            },
        };
        return collisionWorld.CastRay(raycastInput , out raycastHit);
    }
}
