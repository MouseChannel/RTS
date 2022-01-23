using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using FixedMath;

public struct AgentNeighbor : IBufferElementData
{
    public FixedInt distance;
    public Agent agent;
  
}
