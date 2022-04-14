using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FSG.MeshAnimator.ShaderAnimated;

public class TestFunc : MonoBehaviour
{
    public List<ShaderMeshAnimation> Animation;
    // public ShaderMeshAnimation run;
    // public ShaderMeshAnimation idle;
    [SerializeField] GameObject go;
    public int unitCount;
    [SerializeField]
    private Material m;
    // Start is called before the first frame update
    void Start()
    {

        SetupTextureData();
        for (int i = 0; i < unitCount; i++)
        {
            for (int j = 0; j < unitCount; j++)
            {
                Instantiate(go, new Vector3(i, 0, j), Quaternion.identity);


            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void SetupTextureData()
    {
        Debug.Log("Texture");



        // if (!_animTextures.ContainsKey(baseMesh))
        // {
        int totalTextures = 0;
        Vector2Int texSize = Vector2Int.zero;
        foreach (var anim in Animation)
        {



            totalTextures += anim.textures.Length;
            for (int t = 0; t < anim.textures.Length; t++)
            {
                if (anim.textures[t].width > texSize.x)
                    texSize.x = anim.textures[t].width;

                if (anim.textures[t].height > texSize.y)
                    texSize.y = anim.textures[t].height;
            }
        }

        var textureLimit = QualitySettings.masterTextureLimit;
        QualitySettings.masterTextureLimit = 0;
        var copyTextureSupport = SystemInfo.copyTextureSupport;
        Texture2DArray texture2DArray = new Texture2DArray(texSize.x, texSize.y, totalTextures, Animation[0].textures[0].format, false, false);
        texture2DArray.filterMode = FilterMode.Point;
        DontDestroyOnLoad(texture2DArray);
        int index = 0;
        foreach (var anim in Animation)
        {

            for (int t = 0; t < anim.textures.Length; t++)
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
            totalTextures += anim.textures.Length;
        }
        if (copyTextureSupport == UnityEngine.Rendering.CopyTextureSupport.None)
        {
            texture2DArray.Apply(true, true);
        }
        // _animTextures.Add(baseMesh, texture2DArray);
        QualitySettings.masterTextureLimit = textureLimit;


        //     _materialCacheLookup.Clear();
        m.SetTexture("_AnimTextures", texture2DArray);

        // var meshRenderer = GetComponent<MeshRenderer>();
        // List<Material> _materialCacheLookup = new List<Material>();
        // meshRenderer.GetSharedMaterials(_materialCacheLookup);


        // for (int m = 0; m < _materialCacheLookup.Count; m++)
        // {
        //     Material material = _materialCacheLookup[m];
        //     // if (_setMaterials.Contains(material))
        //     //     continue;
        //     Debug.Log("ChangeMAter");
        //     material.SetTexture("_AnimTextures", texture2DArray);
        //     // _setMaterials.Add(material);
        // }


    }

}
