using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using FSG.MeshAnimator.ShaderAnimated;
using Unity.Mathematics;

public class ExposedTransformWindow : EditorWindow
{

    [MenuItem("Window/ExposedTransformWindow")]
    private static void ShowWindow()
    {
        var window = GetWindow<ExposedTransformWindow>();
        window.titleContent = new GUIContent("ExposedTransformWindow");
        window.Show();
    }
    private GameObject prefab;
    private ShaderMeshAnimation shaderMeshAnimation;
    private string fileName = Application.streamingAssetsPath + "/UnitData.csv";
    private List<string> exposedTransform = new List<string>();
    private TextAsset textAsset;
    private string lineData;


    private void OnGUI()
    {
        GUILayout.Label("位置转化器1");
        using (new EditorGUILayout.HorizontalScope())
        {
            prefab = EditorGUILayout.ObjectField("Prefab", prefab, typeof(GameObject), true) as GameObject;
        }
        using (new EditorGUILayout.HorizontalScope())
        {

            shaderMeshAnimation = EditorGUILayout.ObjectField("Animation", shaderMeshAnimation, typeof(ShaderMeshAnimation), true) as ShaderMeshAnimation;
        }
        if (GUILayout.Button("转化", GUILayout.Height(30)))
        {
            // fileName 

            // WriteHeader();
            // exposedTransform.Clear();
            // string writeData = "";
            // foreach (var i in prefab.transform.GetComponentsInChildren<Transform>())
            // {
            //     if (i.TryGetComponent<MeshRenderer>(out MeshRenderer render))
            //     {
            //         AddItem(ref writeData, i);
            //     }
            //     else
            //     {
            //         AddExposedTransform(ref writeData, i);
            //     }

            //     Debug.Log(i.name + " " + i.position);
            // }

            // using (TextWriter tw = new StreamWriter(fileName, true))
            // {
            //     tw.WriteLine(writeData);
            //     tw.Close();
            // }




            // using (TextReader textReader = new StreamReader(fileName))
            // {
            //     // 先读两行没用的
            //     textReader.ReadLine();
            //     textReader.ReadLine();
            //     while (true)
            //     {
            //         var lineString = textReader.ReadLine();
            //         if (lineString.Length == 0 || lineString == null) break;
            //         var lineData = lineString.Split(',');
            //         if (lineData[0].Length == 0)
            //         {
            //             // GetExposedData()
            //         }
            //         else if (lineData[1].Length == 0)
            //         {

            //         }

            //     }





            // }


            ConvertShaderAnimation();


        }

    }
    private void ConvertShaderAnimation()
    {


        var newAnim = new AnimationScriptableObject();



        var oldAnim = shaderMeshAnimation;
        var originPath = AssetDatabase.GetAssetPath(oldAnim);
        var originFolderPath = originPath.Substring(0, originPath.Length - oldAnim.name.Length - 6);
        Debug.Log(originFolderPath);

        AssetDatabase.CreateAsset(newAnim, string.Format("{0}My{1}.asset", originFolderPath, oldAnim.name));

        newAnim.name = oldAnim.name;
        newAnim.animationName = oldAnim.animationName;
        newAnim.exposedObjects = oldAnim.exposedTransforms;
        newAnim.exposedPosition = oldAnim.exposedPosition;
        newAnim.exposedFramePositionData = new ExposedFramePositionData[oldAnim.frameData.Length];
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
                    rotation1 = r,
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
        newAnim.textures = new List<Texture2D>();

        for (int i = 0; i < oldAnim.textures.Length; i++)
        {

            var curTexture = oldAnim.textures[i];
            var newTexture = new Texture2D(curTexture.width, curTexture.height, curTexture.format, curTexture.mipmapCount, false);
            newTexture.name = i.ToString();
            Graphics.CopyTexture(curTexture, newTexture);
            newAnim.textures.Add(newTexture);
            AssetDatabase.AddObjectToAsset(newTexture, newAnim);
        }







        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(newAnim));



    }

    private void AddItem(ref string writeData, Transform i)
    {
        writeData += ",";
        writeData += i.name.ToString();
        writeData += ",";
        writeData += i.position.x.ToString();
        writeData += ",";
        writeData += i.position.y.ToString();
        writeData += ",";
        writeData += i.position.z.ToString();

        writeData += ",";
        writeData += i.rotation.x.ToString();
        writeData += ",";
        writeData += i.rotation.y.ToString();
        writeData += ",";
        writeData += i.rotation.z.ToString();

        writeData += ",";
        writeData += i.lossyScale.x.ToString();
        writeData += ",";
        writeData += i.lossyScale.y.ToString();
        writeData += ",";
        writeData += i.lossyScale.z.ToString();

        writeData += "\n";
    }

    private void WriteHeader()
    {
        using (TextWriter tw = new StreamWriter(fileName, false))
        {
            tw.WriteLine("ExposedName,ItemName,Postion_x,Postion_y,Postion_z,Rotation_x,Rotation_y,Rotation_z,Scale_x,Scale_y,Scale_z");
            tw.Close();
        }

    }
    private void AddExposedTransform(ref string writeData, Transform i)
    {

        writeData += i.name.ToString();
        writeData += ",,";
        writeData += i.position.x.ToString();
        writeData += ",";
        writeData += i.position.y.ToString();
        writeData += ",";
        writeData += i.position.z.ToString();

        writeData += ",";
        writeData += i.rotation.x.ToString();
        writeData += ",";
        writeData += i.rotation.y.ToString();
        writeData += ",";
        writeData += i.rotation.z.ToString();

        writeData += ",";
        writeData += i.lossyScale.x.ToString();
        writeData += ",";
        writeData += i.lossyScale.y.ToString();
        writeData += ",";
        writeData += i.lossyScale.z.ToString();

        writeData += "\n";
    }
}
