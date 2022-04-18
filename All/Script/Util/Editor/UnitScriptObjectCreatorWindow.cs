
using System.ComponentModel;
using UnityEngine;
using UnityEditor;
using FSG.MeshAnimator.ShaderAnimated;
using System.Collections.Generic;
using Unity.Mathematics;
using System;

public class UnitScriptObjectCreatorWindow : EditorWindow
{

    [MenuItem("Window/UnitScriptObjectCreatorWindow")]
    private static void ShowWindow()
    {
        var window = GetWindow<UnitScriptObjectCreatorWindow>();
        window.titleContent = new GUIContent("UnitScriptObjectCreatorWindow");
        window.Show();
    }
    private List<ShaderMeshAnimation> shaderAnimations = new List<ShaderMeshAnimation>();
    private ShaderMeshAnimation shaderMeshAnimation;
    private List<AnimationScriptableObject> myAnimations = new List<AnimationScriptableObject>();
    private List<Mesh> items = new List<Mesh>();
    private Mesh mainMesh;
    private string outputDir = "Asset/";

    private void OnGUI()
    {
        using (new EditorGUILayout.VerticalScope())
        {
            mainMesh = EditorGUILayout.ObjectField("MainMesh  ", mainMesh, typeof(Mesh), true) as Mesh;

        }
        using (new EditorGUILayout.VerticalScope())
        {


            for (int i = 0; i < shaderAnimations.Count; i++)
            {
                bool remove = false;
                GUILayout.BeginHorizontal();
                {
                    shaderAnimations[i] = EditorGUILayout.ObjectField("Shader Animation  " + i, shaderAnimations[i], typeof(ShaderMeshAnimation), true) as ShaderMeshAnimation;
                    if (GUILayout.Button("X", GUILayout.MaxWidth(20)))
                        remove = true;
                }
                GUILayout.EndHorizontal();
                if (remove)
                {
                    shaderAnimations.RemoveAt(i);
                    break;
                }
            }
            if (GUILayout.Button("+ Add Shader Animation"))
                shaderAnimations.Add(null);
        }

        if (GUILayout.Button("生成UnitScriptableObject", GUILayout.Height(30)))
        {
            var newUnitScriptableObject = new UnitScriptableObject();
            AssetDatabase.CreateAsset(newUnitScriptableObject, string.Format("Assets/All/Animation/Tes/newUnitScriptableObject.asset"));
            //Add MainMesh
            AddMainMesh(newUnitScriptableObject);

            //converAnimation
            AddAnimations(newUnitScriptableObject);
            AddExposedTransforms(newUnitScriptableObject);

            AddItems(newUnitScriptableObject);

            EditorUtility.SetDirty(newUnitScriptableObject);
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(newUnitScriptableObject));

            EditorUtility.DisplayDialog("生成完毕", string.Empty, "OK");

        }



    }

    private void AddExposedTransforms(UnitScriptableObject unit)
    {
         if(unit.exposedTransforms == null){
            unit.exposedTransforms = new List<string>();
        }
        unit.exposedTransforms.AddRange(shaderAnimations[0].exposedTransforms);
    }

    private void AddItems(UnitScriptableObject unit)
    {
        var exposedItemsDatas = shaderAnimations[0].exposedItemsDatas;
        for (int i = 0; i < exposedItemsDatas.Count; i++)
        {
            var newMesh = new Mesh();
            newMesh.name = string.Format("Item_{0}", exposedItemsDatas[i].mesh.name);
            TransferMesh(exposedItemsDatas[i].mesh, newMesh);
            if (unit.items == null)
            {
                unit.items = new List<exposedItemsData>();
            }
            var index = 0;
            for (int j = 0; j < shaderAnimations[0].exposedTransforms.Length;j++){
                if(shaderAnimations[0].exposedTransforms[j] == exposedItemsDatas[i].fatherName){
                    index = j;
                    break;
                }                 
            }


            var initialPosition = shaderAnimations[0].exposedPosition[index];

            unit.items.Add(new exposedItemsData
                {
                    mesh = newMesh,
                    position = exposedItemsDatas[i].position,
                    initialPosition =initialPosition,
                    fatherName = exposedItemsDatas[i].fatherName,
                    fatherIndex = unit.exposedTransforms.IndexOf(exposedItemsDatas[i].fatherName)

                });
            EditorUtility.SetDirty(newMesh);
            AssetDatabase.AddObjectToAsset(newMesh, unit);

        }


    }

    private void AddAnimations(UnitScriptableObject newUnitScriptableObject)
    {
        for (int i = 0; i < shaderAnimations.Count; i++)
        {
            shaderMeshAnimation = shaderAnimations[i];
            ConvertShaderAnimation(newUnitScriptableObject);

        }
    }

    private void AddMainMesh(UnitScriptableObject unit)
    {
        var newMesh = new Mesh();
        TransferMesh(mainMesh, newMesh);
        newMesh.name = "MainMesh";
        unit.mainMesh = newMesh;

        AssetDatabase.AddObjectToAsset(newMesh, unit);


    }
    private void TransferMesh(Mesh from, Mesh to)
    {
        to.vertices = from.vertices;
        // to.subMeshCount = from.subMeshCount;
        to.subMeshCount = 1;
        var triangles = new List<int>();
        for (int i = 0; i < from.subMeshCount; i++)
        {
            triangles.AddRange(from.GetTriangles(i));
            to.SetTriangles(from.GetTriangles(i), i);
        }
        to.SetTriangles(triangles, 0);

        to.normals = from.normals;
        to.tangents = from.tangents;
        to.colors = from.colors;
        to.uv = from.uv;
        to.uv2 = from.uv2;
        to.uv3 = from.uv3;
        to.uv4 = from.uv4;
    }



    private void ConvertShaderAnimation(UnitScriptableObject unit)
    {


        var newAnim = new AnimationScriptableObject();



        var oldAnim = shaderMeshAnimation;
        var originPath = AssetDatabase.GetAssetPath(oldAnim);
        var originFolderPath = originPath.Substring(0, originPath.Length - oldAnim.name.Length - 6);
        Debug.Log(originFolderPath);
        AssetDatabase.AddObjectToAsset(newAnim, unit);

        // AssetDatabase.CreateAsset(newAnim, string.Format("Assets/All/Animation/Tes/newUnitScriptableObject.asset" ));

        newAnim.name = oldAnim.name;
        newAnim.animationName = oldAnim.animationName;

        // newAnim.needuseItems = new List<ExposedData>();
        // for (int i = 0; i < oldAnim.exposedTransforms.Length; i++)
        // {
        //     newAnim.needuseItems.Add(new ExposedData
        //     {
        //         exposedName = oldAnim.exposedTransforms[i],
        //     });
        // }

        // newAnim.exposedPosition = oldAnim.exposedPosition;
        newAnim.exposedFramePositionData = new ExposedFramePositionData[oldAnim.frameData.Length];
        //transfer frameData
        for (int i = 0; i < oldAnim.frameData.Length; i++)
        {
            newAnim.exposedFramePositionData[i].singleFrameData = new SingleFrameData[oldAnim.frameData[i].exposedTransforms.Length];
            for (int j = 0; j < oldAnim.frameData[i].exposedTransforms.Length; j++)
            {
                var oldExposedTransforms = oldAnim.frameData[i].exposedTransforms[j];
                Vector3 position = oldExposedTransforms.GetColumn(3);
                Quaternion r;
                var f = oldExposedTransforms.GetColumn(2);
                if (f == Vector4.zero)
                    r = Quaternion.identity;
                else
                {
                    r = Quaternion.LookRotation(f, oldExposedTransforms.GetColumn(1));
                }



                var curFrameData = new SingleFrameData
                {
                    translation = position,
                    rotation = r,

                };
                newAnim.exposedFramePositionData[i].singleFrameData[j] = curFrameData;

            }


        }


        newAnim.vertexCount = oldAnim.vertexCount;
        newAnim.textureCount = oldAnim.textureCount;
        newAnim.textureSize = new int2(oldAnim.textureSize.x, oldAnim.textureSize.y);
        newAnim.animScalar = oldAnim.animScalar;
        newAnim.length = oldAnim.length;
        newAnim.totalFrames = oldAnim.TotalFrames;
        //transfer textures
        newAnim.textures = new List<Texture2D>();

        for (int i = 0; i < oldAnim.textures.Length; i++)
        {

            var curTexture = oldAnim.textures[i];
            var newTexture = new Texture2D(curTexture.width, curTexture.height, curTexture.format, curTexture.mipmapCount, false);
            newTexture.name = string.Format("{0}_{1}", oldAnim.name, i);
            Graphics.CopyTexture(curTexture, newTexture);
            newAnim.textures.Add(newTexture);
            AssetDatabase.AddObjectToAsset(newTexture, newAnim);
        }
        EditorUtility.SetDirty(newAnim);
        if (unit.animations == null)
        {
            unit.animations = new List<AnimationScriptableObject>();
        }
        unit.animations.Add(newAnim);











    }


}
