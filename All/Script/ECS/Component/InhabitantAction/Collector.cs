using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using FixedMath;


public enum CollectorState
{

    idle = 0,
    goToResource = 1,
    working = 2,
    backToStop = 3,


}

public struct Collector : IComponentData
{

    public CollectorState collectorState;
    public int currentResourceStore;
}
