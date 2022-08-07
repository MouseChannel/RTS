
// using System.Collections;
// using System.ComponentModel;
// using UnityEngine;
// using UnityEditor;
// using FSG.MeshAnimator.ShaderAnimated;
// using System.Collections.Generic;
// using Unity.Mathematics;
// using System;

// public class UnitScriptObjectCreatorWindow : EditorWindow
// {

//     [MenuItem("Window/UnitScriptObjectCreatorWindow")]
//     private static void ShowWindow()
//     {
//         var window = GetWindow<UnitScriptObjectCreatorWindow>();
//         window.titleContent = new GUIContent("UnitScriptObjectCreatorWindow");
//         window.Show();
//     }
//     // private List<ShaderMeshAnimation> shaderAnimations = new List<ShaderMeshAnimation>();
//     private ShaderMeshAnimation shaderMeshAnimation;

//     private Mesh mainMesh;
//     private GameObject prefab;
//     private List<GameObject> prefabs = new List<GameObject>();
//     private HashSet<Mesh> animationMeshs = new HashSet<Mesh>();
//     private List<Mesh> hasBeenCreatedMesh = new List<Mesh>();
//     private Dictionary<Mesh, Mesh> meshToMeshDic = new Dictionary<Mesh, Mesh>();
//     private List<Mesh> newMeshs = new List<Mesh>();
//     private List<Texture2D> textures = new List<Texture2D>();
//     private Material material;
//     private List<AnimationScriptableObject> myAnimations = new List<AnimationScriptableObject>();

//     private string outputDir = "Asset/";

//     private void OnGUI()
//     {
//         // using (new EditorGUILayout.VerticalScope())
//         // {
//         //     mainMesh = EditorGUILayout.ObjectField("MainMesh  ", mainMesh, typeof(Mesh), true) as Mesh;


//         // }
//         using (new EditorGUILayout.VerticalScope())
//         {
//             material = EditorGUILayout.ObjectField("Material  ", material, typeof(Material), true) as Material;


//         }
//         using (new EditorGUILayout.VerticalScope())
//         {
//             // prefab = EditorGUILayout.ObjectField("PrefAB  ", prefab, typeof(GameObject), true) as GameObject;


//             for (int i = 0; i < prefabs.Count; i++)
//             {
//                 bool remove = false;
//                 GUILayout.BeginHorizontal();
//                 {
//                     prefabs[i] = EditorGUILayout.ObjectField("prefab  " + i, prefabs[i], typeof(GameObject), true) as GameObject;
//                     if (GUILayout.Button("X", GUILayout.MaxWidth(20)))
//                         remove = true;
//                 }
//                 GUILayout.EndHorizontal();
//                 if (remove)
//                 {
//                     prefabs.RemoveAt(i);
//                     break;
//                 }
//             }
//             if (GUILayout.Button("+ Add prefab"))
//                 prefabs.Add(null);
//         }

//         if (GUILayout.Button("生成UnitScriptableObject", GUILayout.Height(30)))
//         {
//             var newUnitScriptableObject = new UnitScriptableObject();
//             AssetDatabase.CreateAsset(newUnitScriptableObject, string.Format("Assets/All/Animation/Tes/newUnitScriptableObject.asset"));
//             animationMeshs.Clear();
//             hasBeenCreatedMesh.Clear();
//             newMeshs.Clear();
//             myAnimations.Clear();
//             textures.Clear();
//             meshToMeshDic.Clear();
//             newUnitScriptableObject.mainMaterial = material;
//             // var newMaterial = new Material(material);
//             // newMaterial.name = "material";
//             // AssetDatabase.AddObjectToAsset(newMaterial, newUnitScriptableObject);

//             foreach (var ii in prefabs)
//             {
//                 prefab = ii;


//                 var com = prefab.GetComponent<ShaderMeshAnimator>();
//                 foreach (var i in com.meshStored)
//                 {
//                     animationMeshs.Add(i);
//                 }

//                 // AddAnmationMesh(newUnitScriptableObject, com);
//                 AddShaderAnimations(newUnitScriptableObject, com );

//             }
//             foreach (var i in newMeshs)
//             {
//                 AssetDatabase.AddObjectToAsset(i, newUnitScriptableObject);
//             }
//             foreach (var i in myAnimations)
//             {
//                 AssetDatabase.AddObjectToAsset(i, newUnitScriptableObject);
//             }




//             EditorUtility.SetDirty(newUnitScriptableObject);
//             AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(newUnitScriptableObject));

//             EditorUtility.DisplayDialog("生成完毕", string.Empty, "OK");

//         }
//     }

//     private void AddShaderAnimations(UnitScriptableObject unit, ShaderMeshAnimator com )
//     {

//         var shaderAnimations = com.meshAnimations;
//         foreach (var i in shaderAnimations)
//         {
//             var newAnimationScriptable = new AnimationScriptableObject();
//             //mesh stuff
//             if (!meshToMeshDic.ContainsKey(i.mainMesh))
//             {

//                 var newMesh = new Mesh();
//                 TransferMesh(i.mainMesh, newMesh);
//                 newMesh.name = i.mainMesh.name;
//                 meshToMeshDic.Add(i.mainMesh, newMesh);
//                 newMeshs.Add(newMesh);
//                 newAnimationScriptable.mesh = newMesh;
//                 if (unit.animationMeshs == null)
//                 {
//                     unit.animationMeshs = new List<Mesh>();
//                 }
//                 unit.animationMeshs.Add(newMesh);
//                 newAnimationScriptable.meshIndex = unit.animationMeshs.IndexOf(newMesh);
//             }
//             else
//             {
//                 newAnimationScriptable.mesh = meshToMeshDic[i.mainMesh];
//                 newAnimationScriptable.meshIndex = unit.animationMeshs.IndexOf(meshToMeshDic[i.mainMesh]);
//             }
//             //animationScriptableObject Stuff

//             var oldAnim = i;
//             var originPath = AssetDatabase.GetAssetPath(oldAnim);
//             var originFolderPath = originPath.Substring(0, originPath.Length - oldAnim.name.Length - 6);
//             Debug.Log(originFolderPath);
//             myAnimations.Add(newAnimationScriptable);
//             var newAnim = newAnimationScriptable;

//             // AssetDatabase.CreateAsset(newAnim, string.Format("Assets/All/Animation/Tes/newUnitScriptableObject.asset" ));

//             newAnim.name = oldAnim.name;
//             newAnim.animationName = oldAnim.animationName;

//             newAnim.vertexCount = oldAnim.vertexCount;
//             newAnim.textureCount = oldAnim.textureCount;
//             newAnim.textureSize = new int2(oldAnim.textureSize.x, oldAnim.textureSize.y);
//             newAnim.animScalar = oldAnim.animScalar;
//             newAnim.length = oldAnim.length;
//             newAnim.totalFrames = oldAnim.TotalFrames;
//             // newAnim.material = newMaterial;
//             //transfer textures
//             newAnim.textures = new List<Texture2D>();

//             for (int ii = 0; ii < oldAnim.textures.Length; ii++)
//             {
//                 var curTexture = oldAnim.textures[ii];
//                 var newTexture = new Texture2D(curTexture.width, curTexture.height, curTexture.format, curTexture.mipmapCount, false);
//                 newTexture.name = string.Format("{0}_{1}", oldAnim.name, ii);
//                 Graphics.CopyTexture(curTexture, newTexture);
//                 newAnim.textures.Add(newTexture);
//                 AssetDatabase.AddObjectToAsset(newTexture, unit);
//             }
//             EditorUtility.SetDirty(newAnim);
//             if (unit.animations == null)
//             {
//                 unit.animations = new List<AnimationScriptableObject>();
//             }
//             unit.animations.Add(newAnim);
//         }

//     }


//     private void TransferMesh(Mesh from, Mesh to)
//     {
//         to.vertices = from.vertices;
//         // to.subMeshCount = from.subMeshCount;
//         to.subMeshCount = 1;
//         var triangles = new List<int>();
//         for (int i = 0; i < from.subMeshCount; i++)
//         {
//             triangles.AddRange(from.GetTriangles(i));
//             // to.SetTriangles(from.GetTriangles(i), i);
//         }
//         to.SetTriangles(triangles, 0);

//         to.normals = from.normals;
//         to.tangents = from.tangents;
//         to.colors = from.colors;
//         to.uv = from.uv;
//         to.uv2 = from.uv2;
//         to.uv3 = from.uv3;
//         to.uv4 = from.uv4;
//     }





// }
