using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixedMath;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Collections;

public partial  class GridSystem : SystemBase
{

    public static GridSystem Instance;
    private int width;
    private int length;

    private int cellSize;
    public static NativeArray<GridNode>  gridArray;
    private FixedVector3 originPosition;

    protected override void OnCreate()
    {
        Instance = this;

        width = StaticData.gridWidth;
        length = StaticData.gridLength;
        gridArray = new NativeArray<GridNode>(width * width, Allocator.Persistent);
        for (int i = 0; i < length; i++)
        {
            var newGrid = new GridNode();
            newGrid.Init(i);
            gridArray[i] = newGrid;
        }
    }
    protected override void OnDestroy()
    {
        gridArray.Dispose();
    }
    // public override void InitInstance()
    // {


    // }
    public int GetIndex(FixedVector2 agentPosition)
    {
        var x = (agentPosition.X.round).RawInt;
        var y = (agentPosition.Y.round).RawInt;
        ValidateGridPosition(ref x, ref y);
        return y * width + x;
    }



    public void GetXZ(float3 worldPosition, out int x, out int y)
    {
        x = (int)math.round(worldPosition.x);
        y = (int)math.round(worldPosition.z);
        ValidateGridPosition(ref x, ref y);



    }


    public int2 GetXZ(FixedVector2 worldPosition)
    {
        var x = (worldPosition.X.round).RawInt;
        var y = (worldPosition.Y.round).RawInt;
        ValidateGridPosition(ref x, ref y);
        return new int2(x, y);
    }
    public int GetGridIndex(float3 worldPosition)
    {
        FixedVector2 temp = new FixedVector2((FixedInt)worldPosition.x, (FixedInt)worldPosition.z);
        return GetGridIndex(temp);
    }

    public static int GetGridIndex(FixedVector2 worldPosition)
    {
        if (worldPosition == FixedVector2.inVaild)
        {
            return FixedInt.inVaild.RawInt;
        }

        var x = (worldPosition.X.round).RawInt;
        var y = (worldPosition.Y.round).RawInt;
        ValidateGridPosition(ref x, ref y);
        return y * StaticData.gridWidth + x;
    }
    public static void SetGrid(int y, int x)
    {
        // gridArray[y * 512 + x].giz = true;

    }
    public static int GetGridIndexInFOW(FixedVector2 worldPosition)
    {
        var x = (worldPosition.X.round).RawInt + StaticData.gridWidth / 2;
        var y = (worldPosition.Y.round).RawInt + StaticData.gridWidth / 2;
        ValidateGridPosition(ref x, ref y);
        return y * StaticData.gridWidth + x;
    }



    public int GetWidth() => width;
    public int GetLength() => length;

    public NativeArray<GridNode> GetGridArray() => gridArray;


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
    public void SetUnWalkableArea(int2 center, FixedInt radius, int vertices)
    {
        //暂时只会写vertices == 4的情况
        for (FixedInt i = center.x - radius; i <= center.x + radius; i++)
            for (FixedInt j = center.y - radius; j <= center.y + radius; j++)
            {
                var index = GetGridIndex(new FixedVector2(i, j));
                gridArray[index].SetIsWalkable(false);
            }
    }



    private static void ValidateGridPosition(ref int x, ref int y)
    {
        x = math.clamp(x, 0, StaticData.gridWidth - 1);
        y = math.clamp(y, 0, StaticData.gridWidth - 1);
    }

    protected override void OnUpdate()
    {

    }
}
