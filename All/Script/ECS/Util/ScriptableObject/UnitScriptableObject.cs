using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
// using FSG.MeshAnimator.ShaderAnimated;
using Unity.Mathematics;
using System;
using System.Linq;
using UnityEngine.Rendering;

public enum UnitType
{

};
[System.Serializable]
public struct Item
{
    public string name;
    public int2 renderRange;
}

[CreateAssetMenu(menuName = "ScriptableObject/Create MyScriptableObject ")]
public class UnitScriptableObject : ScriptableObject
{
    public Mesh mainMesh;
    public Mesh subMesh;
    public Material mainMaterial;
    public Material subMaterial;
    public UnitType unitType;


    public List<Mesh> animationMeshs;
    public AnimationScriptableObject defaultAnimation;
    public List<Item> renderRange;


    public List<AnimationScriptableObject> animations;


    public Entity CreateItself(EntityManager entityManager)
    {
        var entity = entityManager.CreateEntity(SpawnUtil.UnitArchetype);


        var names = animations.Select(x => x.animationName).ToArray();
        AnimationUtil.AddAnimationList(animations, mainMaterial, renderRange);

        AnimationUtil.PlayAnimation(defaultAnimation.name, entity);
        InitUnitComponent(entityManager, entity);
        return entity;

    }



    public void InitUnitComponent(EntityManager entityManager, Entity entity)
    {
        entityManager.AddSharedComponentData<RenderMesh>(entity,
        new RenderMesh
        {
            mesh = mainMesh,
            material = mainMaterial,
            layerMask = 1,
            castShadows = ShadowCastingMode.On

        });

        entityManager.SetComponentData<Rotation>(entity,
           new Rotation { Value = new float4(0, 0, 0, 1) }
       );
        entityManager.SetComponentData<NonUniformScale>(entity,
            new NonUniformScale { Value = new float3(1, 1, 1) }
        );
        entityManager.SetComponentData<BuiltinMaterialPropertyUnity_WorldTransformParams>(entity,
            new BuiltinMaterialPropertyUnity_WorldTransformParams { Value = new float4(0, 0, 0, 1) }
        );
        entityManager.SetComponentData<BuiltinMaterialPropertyUnity_LightData>(entity,
        new BuiltinMaterialPropertyUnity_LightData { Value = new float4(0, 0, 1, 0) }
        );
        entityManager.SetComponentData<BuiltinMaterialPropertyUnity_RenderingLayer>(entity,
        new BuiltinMaterialPropertyUnity_RenderingLayer { Value = new uint4(1, 0, 0, 0) });
        entityManager.AddComponentData<AnimationData>(entity, new AnimationData
        {
            currentAnimation = defaultAnimation.animBlob

        });
   




    }

    // private void SetAnimtionStuff(Material material)
    // {
    //     var names = animations.Select(x => x.animationName).ToArray();
    //     AnimationUtil.AddAnimationList(animations, material);


    // }

}
