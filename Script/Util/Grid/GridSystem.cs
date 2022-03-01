using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixedMath;
using Unity.Mathematics;




public class GridSystem : Singleton<GridSystem>
{
    private int width;
    private int length;

    private int cellSize;

    private GridNode[] gridArray;
    private FixedVector3 originPosition;

    public override void InitInstance()
    {
        width = ConfigData.gridWidth;
        length = ConfigData.gridLength;
        gridArray = new GridNode[width * width];
        for (int i = 0; i < length; i++)
        {
            gridArray[i].Init(i);
        }

    }
    public int GetIndex(RVO.FixedVector2 agentPosition){
        var x = (agentPosition.X().round).RawInt;
        var y = (agentPosition.Y().round).RawInt;
        ValidateGridPosition(ref x, ref y);
        return y * width + x;
    }
    


    public void GetXZ(float3 worldPosition, out int x, out int y)
    {
        x = (int)math.round(worldPosition.x);
        y = (int)math.round(worldPosition.z);
        ValidateGridPosition(ref x, ref y);



    }

 
    public int2 GetXZ(RVO.FixedVector2 worldPosition)
    {
        var x = (worldPosition.X().round).RawInt;
        var y = (worldPosition.Y().round).RawInt;
        ValidateGridPosition(ref x, ref y);
        return new int2(x, y);
    }
    public int GetGridIndex(float3 worldPosition)
    {
        RVO.FixedVector2 temp = new RVO.FixedVector2((FixedInt)worldPosition.x, (FixedInt)worldPosition.z);
        return GetGridIndex(temp);
    }
 
    public static int GetGridIndex(RVO.FixedVector2 worldPosition)
    {
        var x = (worldPosition.X().round ).RawInt ;
        var y = (worldPosition.Y().round ).RawInt;
        ValidateGridPosition(ref x, ref y);
        return y * ConfigData.gridWidth + x;
    }



    public int GetWidth() => width;
    public int GetLength() => length;

    public GridNode[] GetGridArray() => gridArray;


    public GridNode GetNode(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < width && y < width)
        {
            return gridArray[x * width + y];
        }

        return default(GridNode);

    }
    public void SetNode(int x, int y, GridNode value)
    {
        if (x >= 0 && y >= 0 && x < width && y < width)
            gridArray[x * width + y] = value;
    }

    // public void SetNode(FixedVector3 worldPosition, GridNode value)
    // {
    //     int x, z;
    //     GetXZ(worldPosition, out x, out z);
    //     SetNode(x, z, value);

    // }

    private static void ValidateGridPosition(ref int x, ref int y)
    {
        x = math.clamp(x, 0, ConfigData.gridWidth - 1);
        y = math.clamp(y, 0, ConfigData.gridWidth - 1);
    }
}
