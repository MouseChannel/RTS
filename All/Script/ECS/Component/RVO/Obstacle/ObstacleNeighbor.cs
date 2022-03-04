using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using FixedMath;


namespace RVO{


    public struct ObstacleNeighbor : IComponentData
    {
        public FixedInt distance;
        public  ObstacleVertice obstacle;
        // public IList<KeyValuePair<FixedInt, Agent>> agentNeighbors_  ;
        // public IList<KeyValuePair<FixedInt, Obstacle>> obstacleNeighbors_  ;
    }

}
