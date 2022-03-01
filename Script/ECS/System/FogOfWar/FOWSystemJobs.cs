
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using System.Collections.Generic;

using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;
using System;
[DisableAutoCreation]
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
            int rangeWidth = (range + 1) * 2 - 1;
            int field = rangeWidth * rangeWidth;

        
            NativeArray<Color32> colorBuffer = new NativeArray<Color32>(field, Allocator.Temp);
            NativeArray<Color32> blurBuffer = new NativeArray<Color32>(field, Allocator.Temp);


            for (int y = 0; y < rangeWidth; y++)
            {
                for (int x = 0; x < rangeWidth; x++)
                {
                    var index = GetRangeIndex(x, y, rangeWidth);
                    if (CheckRange(x - range, y - range, range))
                    {
                        colorBuffer[index] = ChangeColor(colorBuffer[index], 'a', 255);
                    }
                    else
                    {
                        colorBuffer[index] = ChangeColor(colorBuffer[index], 'a', 0);
                    }
                }
            }
            #region 障碍物视角遮挡

            #endregion




            #region 使探索过的区域变成半透明
            // for (int i = 0; i < colorBuffer.Length; i++)
            // {
            //     Color32 c = colorBuffer[i];
            //     if (c.r == 0)
            //     {

            //         blurBuffer[i] = ChangeColor(blurBuffer[i], 'a', c.b == 255 ? (byte)120 : (byte)255);
            //     }
            //     else
            //     {
            //         blurBuffer[i] = ChangeColor(blurBuffer[i], 'a', (byte)(255 - c.r));
            //     }
            // }



            #endregion


            for (int i = 0; i < blurBuffer.Length; i++)
            {

                var index = GetMapIndex(i, rangeWidth );

                if (index != -1
                            // && mapBlurBuffer[index].a > blurBuffer[i].a
                            )
                {
                    mapBlurBuffer[index] = colorBuffer[i];
                }
            }



            colorBuffer.Dispose();
            blurBuffer.Dispose();


        }

        public bool CheckRange(int x, int y, int range) => x * x + y * y > range * range;
        public int GetRangeIndex(int x, int y, int width) => y * width + x;

        public int GetMapIndex(int index, int rangeWidth)
        {
            //暂时
            int mapWidth = 100;

            int startPos = fowUnit.gridIndex;
            var y = startPos / mapWidth;
            var x = startPos % mapWidth;

            var yy = index / rangeWidth;
            var xx = index % rangeWidth;

            var resulty = y + yy - rangeWidth / 2;
            var resultx = x + xx - rangeWidth / 2;


            if (resultx < 0 || resultx >= mapWidth || resulty < 0 || resulty >= mapWidth)
                return -1;


            return resulty * mapWidth + resultx;
        }

        public Color32 ChangeColor(Color32 before, char mark, byte value)
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


    }













}
