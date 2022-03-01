using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using RVO;
using Unity.Jobs;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial class FOWSystem : SystemBase
{
    public NativeArray<Color32> colorBuffer = new NativeArray<Color32>(GridSystem.Instance.GetLength(), Allocator.Persistent);
    public NativeArray<Color32> blurBuffer = new NativeArray<Color32>(GridSystem.Instance.GetLength(), Allocator.Persistent);
    public Material blurMat;
    private Texture2D texBuffer;
    private RenderTexture renderBuffer;
    private RenderTexture renderBuffer2;
    private RenderTexture nextTexture;
    private RenderTexture curTexture;
    /// <summary>
    /// 迷雾贴图对外接口
    /// </summary>
    /// <value></value>
    public Texture FogTexture
    {
        get
        {
            return curTexture;
        }
    }


    protected override void OnCreate()
    {
        World.GetOrCreateSystem<FixedStepSimulationSystemGroup>().Timestep = 0.5f;
    }
    protected override void OnDestroy()
    {
        colorBuffer.Dispose();
        blurBuffer.Dispose();

        RenderTexture.ReleaseTemporary(renderBuffer);
        RenderTexture.ReleaseTemporary(renderBuffer2);
        RenderTexture.ReleaseTemporary(nextTexture);
        RenderTexture.ReleaseTemporary(curTexture);

    }
    protected override void OnUpdate()
    {
        FreshFog();
        CalculateFog();


 
        texBuffer.SetPixels32(blurBuffer.ToArray());
        texBuffer.Apply();
        Graphics.Blit(texBuffer, curTexture, blurMat, 0);

        // Graphics.Blit(texBuffer, renderBuffer, blurMat, 0);
        // // for (int i = 0; i < 1; i++)
        // // {
        // Graphics.Blit(renderBuffer, renderBuffer2, blurMat, 0);
        // Graphics.Blit(renderBuffer2, renderBuffer, blurMat, 0);
        // // }
        // Graphics.Blit(renderBuffer, nextTexture);





        // Lerp();



        // var mapWidth = GridSystem.Instance.pathfindingGrid.GetWidth();

        // var mapHeight = GridSystem.Instance.pathfindingGrid.GetHeight();
        // for (int j = 0; j < mapHeight; j++)
        // {
        //     for (int i = 0; i < mapWidth; i++)
        //     {
        //         if (colorBuffer[i].a != 255)
        //         {
        //             Debug.Log("different");
        //         }
        //     }

        // }


    }

    private void CalculateFog()
    {
        NativeList<JobHandle> jobList = new NativeList<JobHandle>(Allocator.Temp);
     
        Entities.ForEach((in FOWUnit fogUnit) =>
        {
            ComputeFog computeFog = new ComputeFog
            {
                mapBlurBuffer = blurBuffer,
                fowUnit = fogUnit
            };
            jobList.Add(computeFog.Schedule());

 

        }).WithoutBurst().Run();
        JobHandle.CompleteAll(jobList);
 
  
    }

    public void InitFOW()
    {
        var mapWidth = GridSystem.Instance.GetWidth();

        var mapHeight = GridSystem.Instance.GetWidth();
        // colorBuffer = new Color32[mapWidth * mapHeight];
        // blurBuffer = new Color32[mapWidth * mapHeight];

        blurMat = new Material(Shader.Find("ImageEffect/AverageBlur"));
        
        texBuffer = new Texture2D(mapWidth, mapHeight, TextureFormat.ARGB32, false);
        texBuffer.wrapMode = TextureWrapMode.Clamp;
        renderBuffer = RenderTexture.GetTemporary((int)(mapWidth * 1.5f), (int)(mapHeight * 1.5f), 0);
        renderBuffer2 = RenderTexture.GetTemporary((int)(mapWidth * 1.5f), (int)(mapHeight * 1.5f), 0);
        nextTexture = RenderTexture.GetTemporary((int)(mapWidth * 1.5f), (int)(mapHeight * 1.5f), 0);
        curTexture = RenderTexture.GetTemporary((int)(mapWidth * 1.5f), (int)(mapHeight * 1.5f), 0);
        for (int i = 0; i < colorBuffer.Length;i++){
            colorBuffer[i] = new Color32(0, 0, 0, 255);

        }

        Debug.Log("InitFog");
    }

    private void FreshFog()
    {
        for (int i = 0; i < colorBuffer.Length; i++)
        {
            var color = colorBuffer[i];
         
            // if (color.r == 255)
            // {
            //     colorBuffer[i] = new Color32(0, color.g, color.b, color.a);
            // }
            blurBuffer[i] = new Color32(0, 0, 0, 255);
            
        }
    }
    private void Lerp()
    {
        Graphics.Blit(curTexture, renderBuffer);
        blurMat.SetTexture("_LastTex", renderBuffer);
        Graphics.Blit(nextTexture, curTexture, blurMat, 1);
    }



}
