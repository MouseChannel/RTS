using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using System;
using FixedMath;

public struct ObstacleVertice : IComponentData ,IEquatable<ObstacleVertice>
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
