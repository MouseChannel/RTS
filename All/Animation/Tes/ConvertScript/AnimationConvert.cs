using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class AnimationConvert : MonoBehaviour, IConvertGameObjectToEntity
{
    public AnimationScriptableObject anim;
    private static Vector4 _shaderTime { get { return Shader.GetGlobalVector("_Time"); } }
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        SetupTextureData();
        dstManager.AddComponentData<_AnimInfo>(entity, new _AnimInfo
        {
            Value = new float4(
                 0,
                 anim.vertexCount,
                 anim.textureSize.x,
                 anim.textureSize.y
             )
        });
        dstManager.AddComponentData<_AnimTimeInfo>(entity, new _AnimTimeInfo
        {
            Value = new Vector4(
                0,
                anim.totalFrames,
                _shaderTime.y,
                 _shaderTime.y + (anim.length))
        });
        dstManager.AddComponentData<_AnimScalar>(entity, new _AnimScalar
        {
            Value = new float3(anim.animScalar)
            // Value = new float3(2,2,2)
        });
        dstManager.AddComponentData<_AnimTextureIndex>(entity, new _AnimTextureIndex { Value = 0 });
        dstManager.AddComponentData<NonUniformScale>(entity, new NonUniformScale { Value = anim.animScalar });

    }




    private void SetupTextureData()
    {
        Debug.Log("Texture");



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
        DontDestroyOnLoad(texture2DArray);
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

        var meshRenderer = GetComponent<MeshRenderer>();
        List<Material> _materialCacheLookup = new List<Material>();
        meshRenderer.GetSharedMaterials(_materialCacheLookup);


        for (int m = 0; m < _materialCacheLookup.Count; m++)
        {
            Material material = _materialCacheLookup[m];
            // if (_setMaterials.Contains(material))
            //     continue;
            Debug.Log("ChangeMAter");
            material.SetTexture("_AnimTextures", texture2DArray);
            // _setMaterials.Add(material);
        }
        // Debug.Log(_animTextures[baseMesh]);

    }


}
