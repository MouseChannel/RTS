using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using System;
using Unity.Rendering;
using Unity.Mathematics;

public partial class ExposedTransformSystem : SystemBase
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
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnUnit();
        }
        // Entities.ForEach((Entity e, in RenderMesh mesh) =>
        // {
        // endSimulationEntityCommandBufferSystem.CreateCommandBuffer().Remove
        // }).Run();
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

        //为什么倒着写，是因为自动的childBuffer是反的
        for (int i = exposedTransforms.Length - 1; i >= 0; i--)
        {
            var child = EntityManager.CreateEntity(SpawnUtil.ExposedTransformArchetype);
#if UNITY_EDITOR
            EntityManager.SetName(child, exposedTransforms[i]);

#endif
            EntityManager.AddComponentData<Parent>(child, new Parent { Value = entity });
            EntityManager.SetComponentData<Translation>(child, new Translation { Value = new float3(anim.exposedPosition[i]) });
            // EntityManager.SetComponentData<Rotation>(child, new Rotation{Value = new quaternion})
        }


    }

    private void SetAnimationData()
    {
        var entity = beSpawnedEntity;
        var anim = beSpawnedUnitSpriptableObject.animations[0];
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
        foreach(var i in beSpawnedUnitSpriptableObject.animations){
            BlobAssetUtil.AddAnimationElement(i.animationName, i);
        }
        
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
