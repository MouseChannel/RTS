using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using FixedMath;
using System;


public struct GridNode: IComparable<GridNode>, IEquatable<GridNode>
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
    public bool giz;

    public int cameFromNodeIndex;
    // public void CalculateFCost(){
    //     fCost = gCost + hCost;
    // }



    public void SetIsWalkable(bool isWalkable)
    {
        this.isWalkable = isWalkable;
    }
    public void Init(int index)
    {

        this.index = index;
        this.x = index % StaticData.gridWidth;
        this.y = index / StaticData.gridWidth;
        isWalkable = true;
        gCost = int.MaxValue;
        cameFromNodeIndex = -1;
    }

    public int CompareTo(GridNode other)
    {
        return other.fCost - fCost;
        //  return other.fCost - fCost;
    }

    public bool Equals(GridNode other)
    {
        return index == other.index;
    }
}



