using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace RVO{


    public struct ObstacleVertice : IBufferElementData
    {
        public FixedVector2 vertice;
}
}
