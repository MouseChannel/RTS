using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using System;
using Unity.Transforms;
using Unity.Mathematics;
using System.Collections.Concurrent;
using UnityEngine.Profiling;
using System.Linq;
using Unity.Collections;
using Unity.Rendering;
using UnityEngine.Rendering;

public static class AnimationUtil
{
    public static ConcurrentDictionary<string, AnimationScriptableObject> animationDic = new ConcurrentDictionary<string, AnimationScriptableObject>();
    public static ConcurrentDictionary<Material, List<Texture2D>> textureDic = new ConcurrentDictionary<Material, List<Texture2D>>();
    public static float crossfadeDelay = 0.3f;

    public static void AddAnimationList(List<AnimationScriptableObject> anims, Material material, List<Item> renderRange)
    {
        if (!textureDic.ContainsKey(material))
        {
            textureDic.TryAdd(material, new List<Texture2D>());
        }
        if (textureDic.TryGetValue(material, out var textureArr))
        {
            var textures = new List<Texture2D>();
            var names = anims.Select(x => x.animationName).ToArray();

            for (int i = 0; i < anims.Count; i++)
            {
                var anim = anims[i];
                var name = names[i];


                var tempTextures = anim.textures;
                //update Animation Index
                anim.textureIndex = textures.Count;

                anim.renderRange = GetAnimRenderRange(anim.renderIndexs, renderRange);

                anim.animBlob = CreateAnimBlobRef(anim, textures.Count);


                animationDic.TryAdd(name, anim);
                textures.AddRange(tempTextures);

            }




            textureArr.AddRange(textures);
            int totalTextures = textureArr.Count;
            Vector2Int texSize = Vector2Int.zero;



            for (int t = 0; t < textures.Count; t++)
            {
                if (textures[t].width > texSize.x)
                    texSize.x = textures[t].width;

                if (textures[t].height > texSize.y)
                    texSize.y = textures[t].height;
            }

            var textureLimit = QualitySettings.masterTextureLimit;
            QualitySettings.masterTextureLimit = 0;
            var copyTextureSupport = SystemInfo.copyTextureSupport;
            Texture2DArray texture2DArray = new Texture2DArray(texSize.x, texSize.y, totalTextures, textures[0].format, false, false);
            texture2DArray.filterMode = FilterMode.Point;



            for (int t = 0; t < textureArr.Count; t++)
            {
                var tex = textureArr[t];
                Graphics.CopyTexture(tex, 0, 0, texture2DArray, t, 0);
            }
            QualitySettings.masterTextureLimit = textureLimit;

            material.SetTexture("_AnimTextures", texture2DArray);
        }





    }

    private static int4x4 GetAnimRenderRange(List<int> renderIndexs, List<Item> renderRange)
    {

        int4x4 res = int4x4.zero;

        var arr4 = AllocateArrayPool<int4>.PullArray(4);



        for (int i = 0; i < renderIndexs.Count; i += 2)
        {
            int2 temp;
            if (i + 1 >= renderIndexs.Count)
                temp = int2.zero;
            else
                temp = renderRange[renderIndexs[i + 1]].renderRange;

            // arr[i] = renderRange[renderIndexs[i]].renderRange;
            arr4[i / 2] = new int4(renderRange[renderIndexs[i]].renderRange, temp);
        }

        res = new int4x4(arr4[0], arr4[1], arr4[2], arr4[3]);
        AllocateArrayPool<int4>.GiveBackToPool(arr4);
        return res;
    }

    private static BlobAssetReference<AnimationBlobElement> CreateAnimBlobRef(AnimationScriptableObject anim, int index)
    {
        BlobBuilder blobBuilder = new BlobBuilder(Allocator.Temp);


        ref AnimationBlobElement animBlob = ref blobBuilder.ConstructRoot<AnimationBlobElement>();
        animBlob.totalFrames = anim.totalFrames;
        animBlob.length = anim.length;
        animBlob.index = index;
        animBlob.textureSize = anim.textureSize;
        animBlob.vertexCount = anim.vertexCount;
        animBlob.framesPerTexture = (int)((anim.textureSize.x * anim.textureSize.y) / (anim.vertexCount * 2));
        animBlob.scale = anim.animScalar;
        // animBlob.randerRange = 


        BlobAssetReference<AnimationBlobElement> animationBlobElementRef
         = blobBuilder.CreateBlobAssetReference<AnimationBlobElement>(Allocator.Persistent);
        blobBuilder.Dispose();
        return animationBlobElementRef;


    }

    public static void AddAnimationElement(string name, AnimationScriptableObject anim, Material material)
    {
        if (animationDic.ContainsKey(name)) return;


        //setTexture
        if (!textureDic.ContainsKey(material))
        {
            textureDic.TryAdd(material, new List<Texture2D>());
        }
        if (textureDic.TryGetValue(material, out var textureArr))
        {
            var textures = anim.textures;
            //update Animation Index
            anim.textureIndex = textures.Count;
            textureArr.AddRange(textures);

            SetupTextures(textureArr, material);
        }
        animationDic.TryAdd(name, anim);




    }
    private static void SetupTextures(List<Texture2D> textureArr, Material material)
    {
        int totalTextures = textureArr.Count;
        Vector2Int texSize = Vector2Int.zero;



        for (int t = 0; t < textureArr.Count; t++)
        {
            if (textureArr[t].width > texSize.x)
                texSize.x = textureArr[t].width;

            if (textureArr[t].height > texSize.y)
                texSize.y = textureArr[t].height;
        }

        var textureLimit = QualitySettings.masterTextureLimit;
        QualitySettings.masterTextureLimit = 0;
        var copyTextureSupport = SystemInfo.copyTextureSupport;
        Texture2DArray texture2DArray = new Texture2DArray(texSize.x, texSize.y, totalTextures, textureArr[0].format, false, false);
        texture2DArray.filterMode = FilterMode.Point;



        for (int t = 0; t < textureArr.Count; t++)
        {
            var tex = textureArr[t];
            Graphics.CopyTexture(tex, 0, 0, texture2DArray, t, 0);
        }
        QualitySettings.masterTextureLimit = textureLimit;

        material.SetTexture("_AnimTextures", texture2DArray);
    }

    public static EntityManager entityManager
    {
        get
        {
            if (manager == default(EntityManager))
            {
                manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            }
            return manager;

        }
    }
    private static EntityManager manager;


    private static void SetAnimationData(AnimationScriptableObject anim, Entity entity)
    {

        entityManager.SetComponentData<_AnimData>(entity, new _AnimData
        {
            Value = new float4x4(
                new float4(0, anim.totalFrames, StaticData._shaderTime.y, StaticData._shaderTime.y + anim.length),
                new float4(anim.textureIndex, 0, 0, 0),
                new float4(0, anim.vertexCount, anim.textureSize.x, anim.textureSize.y),
                new float4(anim.animScalar, 0)
            )
        });
        entityManager.SetComponentData<_RenderRange>(entity, new _RenderRange
        {
            Value = anim.renderRange
        });
    }
    private static void SetAnimationData(AnimationScriptableObject anim, Entity entity, EntityCommandBuffer.ParallelWriter ecbPara, int entityInQueryIndex)
    {

        ecbPara.SetComponent<_AnimData>(entityInQueryIndex, entity, new _AnimData
        {
            Value = new float4x4(
                new float4(0, anim.totalFrames, StaticData.shaderTime.y, StaticData.shaderTime.y + anim.length),
                new float4(anim.textureIndex, 0, 0, 0),
                new float4(0, anim.vertexCount, anim.textureSize.x, anim.textureSize.y),
                new float4(anim.animScalar, 0)
            )
        });
        ecbPara.SetComponent<_RenderRange>(entityInQueryIndex, entity, new _RenderRange { Value = anim.renderRange });


    }




    public static void PlayAnimation(string name, Entity entity)
    {
        if (animationDic.TryGetValue(name, out var anim))
        {
            SetAnimationData(anim, entity);
        }

    }



    public static void PlayAnimation(string name, Entity entity, EntityCommandBuffer.ParallelWriter ecbPara, int entityInQueryIndex)
    {
        if (animationDic.TryGetValue(name, out var anim))
        {
            SetAnimationData(anim, entity, ecbPara, entityInQueryIndex);
        }

    }


    public static void CrossfadeAnimation(string name, Entity entity, AnimationData curAnim)
    {
        if (animationDic.TryGetValue(name, out var anim))
        {
            var animBlob = curAnim.currentAnimation;
            int2 textureSize = animBlob.Value.textureSize;
            int vertexCount = animBlob.Value.vertexCount;
            int framesPerTexture = animBlob.Value.framesPerTexture;
            int localOffset = (int)(curAnim.currentFrame / (float)framesPerTexture);
            int textureIndex = animBlob.Value.index + localOffset;
            int frameOffset = (int)(curAnim.currentFrame % framesPerTexture);
            int pixelOffset = vertexCount * 2 * frameOffset;





            entityManager.SetComponentData<_CrossfadeAnimTextureIndex>(entity, new _CrossfadeAnimTextureIndex
            {
                Value = textureIndex
            });
            entityManager.SetComponentData<_CrossfadeAnimInfo>(entity, new _CrossfadeAnimInfo
            {
                Value = new float4(
                    pixelOffset,
                    vertexCount,
                    textureSize.x,
                    textureSize.y
                )
            });

            entityManager.SetComponentData<_CrossfadeStartTime>(entity, new _CrossfadeStartTime
            {
                Value = StaticData._shaderTime.y
            });
            entityManager.SetComponentData<_CrossfadeEndTime>(entity, new _CrossfadeEndTime
            {
                Value = StaticData._shaderTime.y + 0.1f
            });

            entityManager.SetComponentData<_CrossfadeData>(entity, new _CrossfadeData
            {
                Value = new float4x4(
                    new float4(textureIndex, 0, 0, 0),
                    new float4(pixelOffset, vertexCount, textureSize.x, textureSize.y),
                    new float4(animBlob.Value.scale, 0),
                    new float4(StaticData._shaderTime.y, StaticData._shaderTime.y + crossfadeDelay, 0, 0)
                )
            });




            SetAnimationData(anim, entity);





        }

    }


    public static void CrossfadeAnimation(string name, Entity entity, ref AnimationData curAnim, EntityCommandBuffer.ParallelWriter ecbPara, int entityInQueryIndex)
    {
        if (animationDic.TryGetValue(name, out var anim))
        {


            var animBlob = curAnim.currentAnimation;

            //需要更换模型，则不适用crossfade
            int2 textureSize = animBlob.Value.textureSize;
            int vertexCount = animBlob.Value.vertexCount;
            int framesPerTexture = animBlob.Value.framesPerTexture;
            int indexOffset = (int)(curAnim.currentFrame / framesPerTexture);
            int textureIndex = animBlob.Value.index + indexOffset;
            int frameOffset = (int)(curAnim.currentFrame % framesPerTexture);
            int pixelOffset = vertexCount * 2 * frameOffset;
            Debug.Log(string.Format("textureIndex {0}, frameOffset  {1}  {2} ", textureIndex, frameOffset, name));


            ecbPara.SetComponent<_CrossfadeData>(entityInQueryIndex, entity, new _CrossfadeData
            {
                Value = new float4x4(
                    new float4(textureIndex, 0, 0, 0),
                    new float4(pixelOffset, vertexCount, textureSize.x, textureSize.y),
                    new float4(animBlob.Value.scale, 0),
                    new float4(StaticData.shaderTime.y, StaticData.shaderTime.y + crossfadeDelay, 0, 0)
                )
            });



            SetAnimationData(anim, entity, ecbPara, entityInQueryIndex);
            curAnim.currentAnimation = anim.animBlob;
            curAnim.currentFrame = 0;
            curAnim.currentTime = 0;













        }

    }







}
