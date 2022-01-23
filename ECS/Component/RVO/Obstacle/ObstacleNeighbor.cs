using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using FixedMath;

public struct ObstacleNeighbor : IBufferElementData
{
    public FixedInt distance;
    public  Obstacle obstacle;
    // public IList<KeyValuePair<FixedInt, Agent>> agentNeighbors_  ;
    // public IList<KeyValuePair<FixedInt, Obstacle>> obstacleNeighbors_  ;
}
