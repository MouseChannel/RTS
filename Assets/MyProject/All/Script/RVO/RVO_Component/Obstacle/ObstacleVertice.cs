using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using System;
using FixedMath;

public struct ObstacleVertice : IBufferElementData, IEquatable<ObstacleVertice>
{
    public int next_;
    public int previous_;

    public FixedVector2 direction_;
    public FixedVector2 point_;
    public int obstacleId_;
    public int verticeId_;
    public bool convex_;

    public bool Equals(ObstacleVertice other)
    {
        return obstacleId_ == other.obstacleId_;
    }




    public static bool operator ==(ObstacleVertice a, ObstacleVertice b)
    {
        return a.next_ == b.next_;
    }
    public static bool operator !=(ObstacleVertice a, ObstacleVertice b)
    {
        return a.next_ != b.next_;
    }


}

public struct ObstacleVerticeTreeNode
{
    public int obstacleVertice_Index;
    public int left_index;
    public int right_index;
    public bool IsDefault()
    {
        var tt = default(ObstacleVerticeTreeNode);
        return obstacleVertice_Index == tt.obstacleVertice_Index
        && left_index == tt.left_index
        && right_index == tt.right_index;
    }

}

/// <summary>
/// 
/// a data struct only used in BuildObstacleVertice
/// </summary>
public struct FixedIntPair
{
    private FixedInt a_;
    private FixedInt b_;

    public FixedIntPair(FixedInt a, FixedInt b)
    {
        a_ = a;
        b_ = b;
    }

    public static bool operator <(FixedIntPair pair1, FixedIntPair pair2)
    {
        return pair1.a_ < pair2.a_ || !(pair2.a_ < pair1.a_) && pair1.b_ < pair2.b_;
    }

    public static bool operator <=(FixedIntPair pair1, FixedIntPair pair2)
    {
        return (pair1.a_ == pair2.a_ && pair1.b_ == pair2.b_) || pair1 < pair2;
    }


    public static bool operator >(FixedIntPair pair1, FixedIntPair pair2)
    {
        return !(pair1 <= pair2);
    }


    public static bool operator >=(FixedIntPair pair1, FixedIntPair pair2)
    {
        return !(pair1 < pair2);
    }
}


/// <summary>
/// gameobject 转化前的顶点
/// </summary>
public struct PreObstacleVertice : IBufferElementData
{
    public FixedVector2 vertice;

    // public int verticeCount;
}

/// <summary>
/// only used in Agent calculate ObstacleVerticeNeighbor
/// </summary>
public struct ObstacleVerticeNeighbor : IComponentData
{
    public FixedInt distance;
    public ObstacleVertice obstacle;
    // public IList<KeyValuePair<FixedInt, Agent>> agentNeighbors_  ;
    // public IList<KeyValuePair<FixedInt, Obstacle>> obstacleNeighbors_  ;
}






