using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using System;
using Unity.Rendering;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;

[AlwaysUpdateSystem]

public partial class SpawnSystem : SystemBase
{
    private static Vector4 _shaderTime { get { return Shader.GetGlobalVector("_Time"); } }
    private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBufferSystem;
    private Entity beSpawnedEntity;
    private UnitScriptableObject beSpawnedUnitSpriptableObject;

    protected override void OnCreate()
    {
        endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }
    protected override void OnUpdate()
    {
        AllocateNativeArrayPool<Entity>.LOOOO();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnUnit();
        }

        Entities.WithNone<Agent>()
                .ForEach((Entity e, in ExposedTransformSystemState exposedTransformSystemState) =>
                {
                    for (int i = 0; i < exposedTransformSystemState.count; i++)
                    {

                        var entity = exposedTransformSystemState.GetEntity(i);
                        EntityManager.DestroyEntity(entity);

                    }
                    Debug.Log(exposedTransformSystemState.exposedEntities.ToUInt64());
                    exposedTransformSystemState.Release();

                    EntityManager.RemoveComponent<ExposedTransformSystemState>(e);
                }).WithStructuralChanges().WithoutBurst().Run();


        // Entities.
        // ForEach((in ExposedTransformSystemState exposedTransformSystemState, in AnimationData animationData) => {


        //  }).ScheduleParallel();


        if (Input.GetKeyDown(KeyCode.F1))
        {




            Entities.ForEach((Entity e, in ExposedTransformSystemState exposedTransformSystemState) =>
                {
                    EntityManager.DestroyEntity(e);
                    // Debug.Log((UInt64)(exposedTransformSystemState.exposedEntities));




                })
                .WithStructuralChanges()
                .WithoutBurst().Run();
        }

    }

    private void SpawnUnit()
    {
        beSpawnedUnitSpriptableObject = Resources.Load<UnitScriptableObject>("Unit");
        beSpawnedEntity = EntityManager.CreateEntity(SpawnUtil.UnitArchetype);
        beSpawnedUnitSpriptableObject.SetUnitComponent(EntityManager, beSpawnedEntity);


        SetupTextureData();
        SetAnimationData();
        SpawnExposedTransform();
        // SpawnItems();



    }

    private Entity SpawnItems()
    {
        var item = EntityManager.CreateEntity(SpawnUtil.ItemArchetype);
        var itemData = beSpawnedUnitSpriptableObject.items[0];
        EntityManager.AddSharedComponentData<RenderMesh>(item,
        new RenderMesh
        {
            mesh = itemData.mesh,
            material = beSpawnedUnitSpriptableObject.subMaterial,

            layerMask = 1
        });
        EntityManager.SetComponentData<NonUniformScale>(item,
           new NonUniformScale { Value = new float3(1, 1, 1) }
       );
        EntityManager.SetComponentData<Rotation>(item,
           new Rotation { Value = new float4(0, 0, 0, 1) }
       );
        EntityManager.SetComponentData<BuiltinMaterialPropertyUnity_WorldTransformParams>(item,
            new BuiltinMaterialPropertyUnity_WorldTransformParams { Value = new float4(0, 0, 0, 1) }
        );
        EntityManager.SetComponentData<BuiltinMaterialPropertyUnity_LightData>(item,
        new BuiltinMaterialPropertyUnity_LightData { Value = new float4(0, 0, 1, 0) }
        );
        EntityManager.SetComponentData<BuiltinMaterialPropertyUnity_RenderingLayer>(item,
        new BuiltinMaterialPropertyUnity_RenderingLayer { Value = new uint4(1, 0, 0, 0) });



        // EntityManager.AddComponentData<Parent>(item, new Parent { Value = father });
        EntityManager.SetComponentData<Translation>(item, new Translation { Value = itemData.GetPosition() });
        EntityManager.SetComponentData<Rotation>(item, new Rotation { Value = itemData.GetRotation() });
        EntityManager.SetComponentData<NonUniformScale>(item, new NonUniformScale { Value = itemData.GetScale() });
        return item;

    }

    private void SpawnExposedTransform()
    {
        var entity = beSpawnedEntity;
        // var anim = beSpawnedUnitSpriptableObject.ex;
        var exposedTransforms = beSpawnedUnitSpriptableObject.exposedTransforms;
        // unsafe
        // {
        AllocateNativeArrayPool.PullArray<Entity>(exposedTransforms.Count + 1, out var transformsEntitys, out var pointer);






        //为什么倒着写，是因为自动的childBuffer是反的
        for (int i = exposedTransforms.Count -1 ; i >= 0; i--)
        {
            var child = EntityManager.CreateEntity(SpawnUtil.ExposedTransformArchetype);
            
            transformsEntitys[i] = child;
#if UNITY_EDITOR
            EntityManager.SetName(child, exposedTransforms[i]);

#endif

            // EntityManager.SetComponentData<Translation>(child, new Translation { Value = new float3(anim.exposedPosition[i]) });
            // EntityManager.SetComponentData<Rotation>(child, new Rotation{Value = new quaternion})
        }
        transformsEntitys[1] = SpawnItems();


        EntityManager.AddComponentData<ExposedTransformSystemState>(entity, new ExposedTransformSystemState
        {
            exposedEntities = pointer,
            count = transformsEntitys.Length
        });
        EntityManager.AddComponent<Agent>(entity);

        // }
        // transformsEntitys.
        // }


    }

    private void SetAnimationData()
    {
        var entity = beSpawnedEntity;
        var anim = beSpawnedUnitSpriptableObject.animations[0];
        //register animation blob asset
        foreach (var i in beSpawnedUnitSpriptableObject.animations)
        {
            BlobAssetUtil.AddAnimationElement(i.animationName, i);
        }
        EntityManager.AddComponentData<AnimationData>(beSpawnedEntity, new AnimationData
        {
            currentAnimation = BlobAssetUtil.animationBlobDic[anim.animationName]
        });
        EntityManager.AddComponentData<_AnimInfo>(entity, new _AnimInfo
        {
            Value = new float4(
               0,
               anim.vertexCount,
               anim.textureSize.x,
               anim.textureSize.y
           )
        });
        EntityManager.AddComponentData<_AnimTimeInfo>(entity, new _AnimTimeInfo
        {
            Value = new Vector4(
                0,
                anim.totalFrames,
                _shaderTime.y,
                 _shaderTime.y + (anim.length))
        });

        EntityManager.AddComponentData<_AnimTextureIndex>(entity, new _AnimTextureIndex { Value = 0 });
        EntityManager.AddComponentData<NonUniformScale>(entity, new NonUniformScale { Value = anim.animScalar });


    }

    private void SetupTextureData()
    {


        var anim = beSpawnedUnitSpriptableObject.animations[0];
        var material = beSpawnedUnitSpriptableObject.mainMaterial;

        // if (!_animTextures.ContainsKey(baseMesh))
        // {
        int totalTextures = 0;
        Vector2Int texSize = Vector2Int.zero;


        totalTextures += anim.textures.Count;
        for (int t = 0; t < anim.textures.Count; t++)
        {
            if (anim.textures[t].width > texSize.x)
                texSize.x = anim.textures[t].width;

            if (anim.textures[t].height > texSize.y)
                texSize.y = anim.textures[t].height;
        }

        var textureLimit = QualitySettings.masterTextureLimit;
        QualitySettings.masterTextureLimit = 0;
        var copyTextureSupport = SystemInfo.copyTextureSupport;
        Texture2DArray texture2DArray = new Texture2DArray(texSize.x, texSize.y, totalTextures, anim.textures[0].format, false, false);
        texture2DArray.filterMode = FilterMode.Point;
        // DontDestroyOnLoad(texture2DArray);
        int index = 0;


        for (int t = 0; t < anim.textures.Count; t++)
        {
            var tex = anim.textures[t];
            if (copyTextureSupport != UnityEngine.Rendering.CopyTextureSupport.None)
            {
                Graphics.CopyTexture(tex, 0, 0, texture2DArray, index, 0);
            }
            else
            {
                texture2DArray.SetPixels(tex.GetPixels(0), index);
            }
            index++;
        }
        totalTextures += anim.textures.Count;

        if (copyTextureSupport == UnityEngine.Rendering.CopyTextureSupport.None)
        {
            texture2DArray.Apply(true, true);
        }
        // _animTextures.Add(baseMesh, texture2DArray);
        QualitySettings.masterTextureLimit = textureLimit;

        //     _materialCacheLookup.Clear();

        // var meshRenderer = GetComponent<MeshRenderer>();
        // List<Material> _materialCacheLookup = new List<Material>();
        // meshRenderer.GetSharedMaterials(_materialCacheLookup);

        material.SetTexture("_AnimTextures", texture2DArray);
        // for (int m = 0; m < _materialCacheLookup.Count; m++)
        // {
        //     Material material = _materialCacheLookup[m];
        //     // if (_setMaterials.Contains(material))
        //     //     continue;
        //     Debug.Log("ChangeMAter");

        //     // _setMaterials.Add(material);
        // }
        // Debug.Log(_animTextures[baseMesh]);

    }



}
