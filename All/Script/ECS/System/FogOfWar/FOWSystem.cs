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
using UnityEngine.Profiling;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[DisableAutoCreation]

public partial class FOWSystem : SystemBase
{
    // public NativeArray<UInt32> colorBuffer = new NativeArray<UInt32>(GridSystem.Instance.GetLength() / 32, Allocator.Persistent);
    public NativeArray<Color32> blurBuffer;
    //  = new NativeArray<Color32>(GridSystem.Instance.GetLength(), Allocator.Persistent);
    public UnsafeList<int> lastVisiableArea;
    // = new UnsafeList<int>(GridSystem.Instance.GetLength(), Allocator.Persistent);
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
        blurBuffer = new NativeArray<Color32>(GridSystem.Instance.GetLength(), Allocator.Persistent);
        lastVisiableArea = new UnsafeList<int>(GridSystem.Instance.GetLength(), Allocator.Persistent);
        World.GetOrCreateSystem<FixedStepSimulationSystemGroup>().Timestep = 0.5f;
        kDTreeSystem = World.GetOrCreateSystem<KDTreeSystem>();
    }
    protected override void OnDestroy()
    {
        // colorBuffer.Dispose();
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



    }

    private void CalculateFog()
    {
        NativeList<JobHandle> jobList = new NativeList<JobHandle>(Allocator.Temp);
        NativeArray<ObstacleVertice> obstacles_ = new NativeArray<ObstacleVertice>(kDTreeSystem.obstacleVertices_, Allocator.TempJob);
        NativeArray<ObstacleVerticeTreeNode> obstacleTree_ = new NativeArray<ObstacleVerticeTreeNode>(kDTreeSystem.obstacleVerticesTree_, Allocator.TempJob);
        ObstacleVerticeTreeNode obstacleTreeRoot = kDTreeSystem.obstacleVerticesTreeRoot;


        UnsafeHashSet<int> visiableArea = new UnsafeHashSet<int>(blurBuffer.Length, Allocator.TempJob);
        var setParaWriter = visiableArea.AsParallelWriter();

        NativeList<FOWUnit> fogUnits = new NativeList<FOWUnit>(Allocator.Temp);
        Entities.ForEach((in FOWUnit fogUnit) =>
        {
            ComputeFogJob computeFogJob = new ComputeFogJob
            {

                fowUnit = fogUnit,
                obstacles_ = obstacles_,
                obstacleTree_ = obstacleTree_,
                obstacleTreeRoot = obstacleTreeRoot,
                setParaWriter = setParaWriter,




            };

      

            jobList.Add(computeFogJob.Schedule());


            fogUnits.Add(fogUnit);



        }).WithoutBurst().Run();
        JobHandle.CompleteAll(jobList);
  
 

        lastVisiableArea.Clear();

        // var visiableAreaArr = visiableArea.ToNativeArray(Allocator.TempJob);
        var setParallelWriter = lastVisiableArea.AsParallelWriter();
        new SetFogPixelJobParallel
        {
            lastVisiableArea = setParallelWriter,
            visiableAreaArr = visiableArea.ToNativeArray(Allocator.TempJob),
            blurBuffer = blurBuffer
        }.Schedule(visiableArea.Count(), 32).Complete();
 
        obstacles_.Dispose();
        obstacleTree_.Dispose();

        jobList.Dispose();
        visiableArea.Dispose();


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
        for (int i = 0; i < blurBuffer.Length; i++)
        {
            blurBuffer[i] = new Color32(0, 0, 0, 222);

        }


    }

    private void FreshFog()
    {

        FreshJobParallel freshJobParallel = new FreshJobParallel
        {
            blurBuffer = blurBuffer,
            lastVisiableArea = lastVisiableArea
        };
        freshJobParallel.Schedule(lastVisiableArea.Length, 32).Complete();

    }





    private void Lerp()
    {
        Graphics.Blit(curTexture, renderBuffer);
        blurMat.SetTexture("_LastTex", renderBuffer);
        Graphics.Blit(nextTexture, curTexture, blurMat, 1);
    }



}
