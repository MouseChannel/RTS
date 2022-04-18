using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
[System.Serializable]
public struct ExposedFramePositionData
{

 
    public SingleFrameData[] singleFrameData;
 

}
[System.Serializable]
public struct SingleFrameData
{
    public float3 translation;
    public quaternion rotation;
    // public Quaternion rotation1;
}

[System.Serializable]
public struct ExposedData{
    public string exposedName;
    // public List<Mesh> hookedItem;
}






[CreateAssetMenu(menuName = "ScriptableObject/Create MySc eObject ")]
public class AnimationScriptableObject : ScriptableObject
{
    public string animationName;
    // public string[] exposedObjects;
    public List<int> needuseItemsIndex;
    /// <summary>
    /// 一维是frame，二维是exposedTransform
    /// </summary>
    public ExposedFramePositionData[] exposedFramePositionData;


    public int vertexCount;
    public int totalFrames;
    public float3 animScalar;
    public float length;
    public int2 textureSize;
    public int textureCount;
    public List<Texture2D> textures;



}
