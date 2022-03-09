using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
 
using Unity.Jobs;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using FixedMath;
using System;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]

public partial class FOWSystem : SystemBase
{
    public NativeArray<Color32> colorBuffer = new NativeArray<Color32>(GridSystem.Instance.GetLength(), Allocator.Persistent);
    public NativeArray<Color32> blurBuffer = new NativeArray<Color32>(GridSystem.Instance.GetLength(), Allocator.Persistent);
    public NativeList<int> lastVisiableArea = new NativeList<int> ( Allocator.Persistent);
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

    private KDTreeSystem kDTreeSystem;


    protected override void OnCreate()
    {
        World.GetOrCreateSystem<FixedStepSimulationSystemGroup>().Timestep = 0.5f;
        kDTreeSystem = World.GetOrCreateSystem<KDTreeSystem>();
    }
    protected override void OnDestroy()
    {
        colorBuffer.Dispose();
        blurBuffer.Dispose();
        lastVisiableArea.Dispose();

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



    }

    private void CalculateFog()
    {
        NativeList<JobHandle> jobList = new NativeList<JobHandle>(Allocator.Temp);
        NativeArray<ObstacleVertice> obstacles_ = new NativeArray<ObstacleVertice>(kDTreeSystem.obstacleVertices_, Allocator.TempJob);
        NativeArray<ObstacleVerticeTreeNode> obstacleTree_ = new NativeArray<ObstacleVerticeTreeNode>(kDTreeSystem.obstacleVerticesTree_, Allocator.TempJob);
        ObstacleVerticeTreeNode obstacleTreeRoot = kDTreeSystem.obstacleVerticesTreeRoot;


        UnsafeHashSet<int> visiableArea = new UnsafeHashSet<int>(colorBuffer.Length, Allocator.TempJob);
        var setParaWriter = visiableArea.AsParallelWriter();
        Entities.ForEach((in FOWUnit fogUnit) =>
        {
            ComputeFog computeFog = new ComputeFog
            {
    
                fowUnit = fogUnit,
                obstacles_ = obstacles_,
                obstacleTree_ = obstacleTree_,
                obstacleTreeRoot = obstacleTreeRoot,
                setParaWriter = setParaWriter


            };
            // computeFog.Run();
            jobList.Add(computeFog.Schedule());

           

        }).WithoutBurst().Run();
        JobHandle.CompleteAll(jobList);

 
        lastVisiableArea.Clear();
 
        foreach (var j in visiableArea)
        {
            lastVisiableArea.Add(j);
            blurBuffer[j] = new Color32(0, 0, 0, 0);
        }
        obstacles_.Dispose();
        obstacleTree_.Dispose();

        jobList.Dispose();
        visiableArea.Dispose();


    }

    //----------



    //------------

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
        for (int i = 0; i < blurBuffer.Length; i++)
        {
            blurBuffer[i] = new Color32(0, 0, 0, 255);

        }

        Debug.Log("InitFog");
    }

    private void FreshFog()
    {
        FreshJob freshJob = new FreshJob
        {
            blurBuffer = blurBuffer,
            lastVisiableArea = lastVisiableArea
        };
        freshJob.Run();

        // foreach(var i in lastVisiableArea){
        //     blurBuffer[i] = new Color32(0, 0, 0, 252);
        // }
    }




    private void Lerp()
    {
        Graphics.Blit(curTexture, renderBuffer);
        blurMat.SetTexture("_LastTex", renderBuffer);
        Graphics.Blit(nextTexture, curTexture, blurMat, 1);
    }



}
