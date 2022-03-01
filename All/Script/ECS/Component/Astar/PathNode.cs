using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using FixedMath;



public struct GridNode
{
    public int x;
    public int y;

    public int index;

    public int gCost;
    public int hCost;
    public int fCost;
    // {
    //     get => gCost + hCost;
    // }

    public bool isWalkable;

    public int cameFromNodeIndex;
    public void CalculateFCost(){
        fCost = gCost + hCost;
    }



    public void SetIsWalkable(bool isWalkable)
    {
        this.isWalkable = isWalkable;
    }
    public void Init(int index)
    {

        this.index = index;
        this.x = index % ConfigData.gridWidth;
        this.y = index / ConfigData.gridWidth;
        isWalkable = true;
        gCost = int.MaxValue;
        cameFromNodeIndex = -1;
    }
}


public struct PathPosition : IBufferElementData
{
    public int2 position;
}
