
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using System.Collections.Generic;

using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;
using System;

public partial class FOWSystem
{
    [BurstCompile]
    public struct ComputeFog : IJob
    {
        [NativeDisableContainerSafetyRestriction]
        public NativeArray<Color32> mapBlurBuffer;
        [ReadOnly] public FOWUnit fowUnit;

        public void Execute()
        { 
            int range = fowUnit.range;
            int field = (2 * range) * (2 * range);

            NativeArray<Color32> colorBuffer = new NativeArray<Color32>(field, Allocator.Temp);
            NativeArray<Color32> blurBuffer = new NativeArray<Color32>(field, Allocator.Temp);
  
            for (int i = 0; i < 2 * range; i++)
            {
                for (int j = 0; j < 2 * range; j++)
                {
                    if ((i - range) * (i - range) + (j - range) * (j - range) > range * range) continue;
                    var index = GetRangeIndex(i, j, range);
                    colorBuffer[index] = ChangeColor(colorBuffer[index], 'r', 255);

                    colorBuffer[index] = ChangeColor(colorBuffer[index], 'b', 255);
                }
            }
             
            #region 高斯模糊
            for (int i = 0; i < colorBuffer.Length; i++)
            {
                Color32 c = colorBuffer[i];
                if (c.r == 0)
                {
                    blurBuffer[i] = ChangeColor(blurBuffer[i], 'a', c.b == 255 ? (byte)120 : (byte)255);
                }
                else
                {
                    blurBuffer[i] = ChangeColor(blurBuffer[i], 'a', (byte)(255 - c.r));
                }
            }



            #endregion
            int startPos = fowUnit.gridIndex;
            for (int i = 0; i < blurBuffer.Length; i++)
            {
                var index = GetMapIndex(i, range, startPos);
                if(index != -1)
                    mapBlurBuffer[index] = blurBuffer[i];
            }


            // texBuffer.SetPixels32(blurBuffer);
            // texBuffer.Apply();
            // Graphics.Blit(texBuffer, renderBuffer, blurMat, 0);
            // // for (int i = 0; i < 1; i++)
            // // {
            // Graphics.Blit(renderBuffer, renderBuffer2, blurMat, 0);
            // Graphics.Blit(renderBuffer2, renderBuffer, blurMat, 0);
            // // }
            // Graphics.Blit(renderBuffer, nextTexture);
            colorBuffer.Dispose();
            blurBuffer.Dispose();
          

        }
    }

    public static int GetRangeIndex(int x, int y, int width) => y * width + x;
    public static int GetMapIndex(int index, int range, int startPos)
    {
        int mapWidth = 100;
        var y = startPos / mapWidth;
        var x = startPos % mapWidth;

        var yy = index / range;
        var xx = index % range;

        var resulty = y + yy - range;
        var resultx = x + xx - range;


        if (resultx < 0 || resultx >= mapWidth || resulty < 0 || resulty >= mapWidth)
            return -1;
        return y * mapWidth + x;
    }
    public static Color32 ChangeColor(Color32 before, char mark, byte value)
    {

        switch (mark)
        {
            case 'r':
                before.r = value;

                break;
            case 'g':
                before.g = value;
                break;
            case 'b':
                before.b = value;
                break;
            case 'a':
                before.a = value;
                break;
        }
        return before;
    }






    [BurstCompile]
    public struct TestJob : IJob
    {
        [NativeDisableContainerSafetyRestriction]
        public NativeArray<int> test;
        public void Execute()
        {
            test[0] = 111;
            

        }
    }



}
