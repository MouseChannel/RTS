using System.Collections;
using System.Collections.Generic;

using Unity.Entities;

using FixedMath;

public struct Obstacle : IComponentData
{
    public int faction;
    public FixedVector2 position_;
    public int id_;



}
public struct ObstacleTreeNode
{
    public int begin_;
    public int end_;
    public int left_;
    public int right_;
    public FixedInt maxX_;
    public FixedInt maxY_;
    public FixedInt minX_;
    public FixedInt minY_;
}
