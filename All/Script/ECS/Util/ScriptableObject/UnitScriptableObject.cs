using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using FSG.MeshAnimator.ShaderAnimated;
using Unity.Mathematics;

public enum UnitType
{

};
[System.Serializable]
public struct exposedItemsData
{
    public Mesh mesh;
    /// <summary>
    /// 第一列position， 第二列rotation， 第三列scale
    /// </summary>
    public float3x3 position;
    public float3 initialPosition;
    public string fatherName;
    
    public int fatherIndex;
    public float3 GetPosition()
    {
        return position.c0 + initialPosition;
    }
    public quaternion GetRotation()
    {
        return quaternion.Euler(position.c1);
    }
    public float3 GetScale()
    {
        return position.c2;
    }


}
[CreateAssetMenu(menuName = "ScriptableObject/Create MyScriptableObject ")]
public class UnitScriptableObject : ScriptableObject
{
    // public static Dictionary<Mesh,>
    private EntityArchetype archetype;

    public EntityArchetype Archetype
    {
        get
        {
            if (World.DefaultGameObjectInjectionWorld == null)
            {
                archetype = default(EntityArchetype);
                return archetype;
            }
            if (archetype != default(EntityArchetype)) return archetype;

            archetype = World.DefaultGameObjectInjectionWorld.EntityManager.CreateArchetype(
                typeof(RenderMesh),
                typeof(LocalToWorld),
                typeof(NonUniformScale),
                typeof(Translation),
                typeof(Rotation),
                typeof(RenderBounds),
                typeof(BuiltinMaterialPropertyUnity_WorldTransformParams),
                typeof(BlendProbeTag),
                typeof(PerInstanceCullingTag),
                typeof(WorldToLocal_Tag),
                typeof(BuiltinMaterialPropertyUnity_RenderingLayer),
                typeof(BuiltinMaterialPropertyUnity_LightData),

                typeof(_AnimInfo),
                typeof(_AnimTimeInfo),
                typeof(_AnimTextureIndex),
                typeof(_CrossfadeAnimInfo),
                typeof(_CrossfadeAnimTextureIndex),
                typeof(_CrossfadeStartTime),
                typeof(_CrossfadeEndTime)
            );
            return archetype;
        }
        set
        {
            archetype = value;
        }
    }
    public Mesh mainMesh;
    public Material mainMaterial;
    public Material subMaterial;
    public UnitType unitType;

    public EntityArchetype ExposedTransformArchetype
    {
        get
        {
            if (World.DefaultGameObjectInjectionWorld == null)
            {
                exposedTransformArchetype = default(EntityArchetype);
                return exposedTransformArchetype;
            }
            if (exposedTransformArchetype != default(EntityArchetype)) return exposedTransformArchetype;

            exposedTransformArchetype = World.DefaultGameObjectInjectionWorld.EntityManager.CreateArchetype(
                typeof(LocalToWorld),
                typeof(LocalToParent),
                typeof(Translation),
                typeof(Rotation)

            );
            return exposedTransformArchetype;
        }
        set
        {
            exposedTransformArchetype = value;
        }
    }
    private EntityArchetype exposedTransformArchetype;
    public List<string> exposedTransforms;
    public List<exposedItemsData> items;

    public List<AnimationScriptableObject> animations;





    void OnEnable()
    {


        // var animationSet = BlobAssetConversionSystem.animationSet;
        // foreach (var i in animations)
        // {
        // animationSet.Add(i);

        // }
        // Debug.Log(animationSet.Count);
    }
    public void SetUnitComponent(EntityManager entityManager, Entity entity)
    {
        entityManager.AddSharedComponentData<RenderMesh>(entity,
        new RenderMesh
        {
            mesh = mainMesh,
            material = mainMaterial,
            layerMask = 1
        });
        entityManager.SetComponentData<NonUniformScale>(entity,
           new NonUniformScale { Value = new float3(1, 1, 1) }
       );
        entityManager.SetComponentData<Rotation>(entity,
           new Rotation { Value = new float4(0, 0, 0, 1) }
       );
        entityManager.SetComponentData<BuiltinMaterialPropertyUnity_WorldTransformParams>(entity,
            new BuiltinMaterialPropertyUnity_WorldTransformParams { Value = new float4(0, 0, 0, 1) }
        );
        entityManager.SetComponentData<BuiltinMaterialPropertyUnity_LightData>(entity,
        new BuiltinMaterialPropertyUnity_LightData { Value = new float4(0, 0, 1, 0) }
        );
        entityManager.SetComponentData<BuiltinMaterialPropertyUnity_RenderingLayer>(entity,
        new BuiltinMaterialPropertyUnity_RenderingLayer { Value = new uint4(1, 0, 0, 0) });


    }


    public void SpawnExposedTransform(EntityManager entityManager, Entity entity, string name)
    {

        var child = entityManager.CreateEntity(ExposedTransformArchetype);
#if UNITY_EDITOR
        entityManager.SetName(child, name);
#endif

        entityManager.AddComponentData<Parent>(child, new Parent { Value = entity });
    }

}
