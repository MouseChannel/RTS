using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Rendering;
using System.Collections.Generic;
using System;

public partial class SpawnSystem : ServiceSystem
{
    private Dictionary<Mesh, bool> existMesh = new Dictionary<Mesh, bool>();

    protected override void OnUpdate()
    {

    }
    public Entity SpawnUnit(UnitScriptableObject unitScriptableObject)
    {
        var mesh = unitScriptableObject.mainMesh;
        var material = unitScriptableObject.mainMaterial;
        var archetype = unitScriptableObject.Archetype;


        Entity entity = EntityManager.CreateEntity(archetype);
        EntityManager.AddSharedComponentData<RenderMesh>(entity,
            new RenderMesh
            {
                mesh = mesh,
                material = material,
                receiveShadows = true,
                layerMask = 1

            }
        );
        EntityManager.SetComponentData<NonUniformScale>(entity,
            new NonUniformScale { Value = new float3(1, 1, 1) }
        );
        // EntityManager.SetComponentData<BuiltinMaterialPropertyUnity_WorldTransformParams>(entity,
        //     new BuiltinMaterialPropertyUnity_WorldTransformParams { Value = new float4(0, 0, 0, 1) }
        // );
        EntityManager.SetComponentData<BuiltinMaterialPropertyUnity_LightData>(entity,
        new BuiltinMaterialPropertyUnity_LightData { Value = new float4(0, 0, 1, 0) }
        );
        EntityManager.SetComponentData<BuiltinMaterialPropertyUnity_RenderingLayer>(entity,
        new BuiltinMaterialPropertyUnity_RenderingLayer { Value = new uint4(1, 0, 0, 0) }
        );
        SetAnimationData(unitScriptableObject);


        return entity;



    }

    private void SetAnimationData(UnitScriptableObject unitScriptableObject)
    {
        var mesh = unitScriptableObject.mainMesh;
        if (existMesh.ContainsKey(mesh)) return;
        var material = unitScriptableObject.mainMaterial;
        var animations = unitScriptableObject.animations;

        existMesh[mesh] = true;





    }
}
