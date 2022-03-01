using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using FixedMath;

public struct Obstacle : IComponentData
{
    public int next_;
    public int previous_;
    // public Obstacle next_;
    // public Obstacle previous_;
    public FixedVector2 direction_;
    public FixedVector2 point_;
    public int id_;
    public bool convex_;

    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override string ToString()
    {
        return base.ToString();
    }

    public static bool operator ==(Obstacle a, Obstacle b)
    {
        return a.next_ == b.next_;
    }
    public static bool operator !=(Obstacle a, Obstacle b)
    {
        return a.next_ != b.next_;
    }


}
