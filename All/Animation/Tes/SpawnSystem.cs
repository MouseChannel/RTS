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
    protected   override void OnUpdate()
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
                    AllocateNativeArrayPool.GiveBackToPool(exposedTransformSystemState.pointerType, exposedTransformSystemState.exposedEntities);
                    EntityManager.RemoveComponent<ExposedTransformSystemState>(e);
                }).WithStructuralChanges().WithoutBurst().Run();


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



    }

    private void SpawnExposedTransform()
    {
        var entity = beSpawnedEntity;
        var anim = beSpawnedUnitSpriptableObject.animations[0];
        var exposedTransforms = anim.exposedObjects;
        // unsafe
        // {
            AllocateNativeArrayPool.PullArray<Entity>(exposedTransforms.Length, out var transformsEntitys, out var pointer);






            //为什么倒着写，是因为自动的childBuffer是反的
            for (int i = exposedTransforms.Length - 1; i >= 0; i--)
            {
                var child = EntityManager.CreateEntity(SpawnUtil.ExposedTransformArchetype);
                transformsEntitys[i] = child;
#if UNITY_EDITOR
                EntityManager.SetName(child, exposedTransforms[i]);

#endif

                EntityManager.SetComponentData<Translation>(child, new Translation { Value = new float3(anim.exposedPosition[i]) });
                // EntityManager.SetComponentData<Rotation>(child, new Rotation{Value = new quaternion})
            }


            EntityManager.AddComponentData<ExposedTransformSystemState>(entity, new ExposedTransformSystemState
            {
                exposedEntities = pointer,
                count = exposedTransforms.Length
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
