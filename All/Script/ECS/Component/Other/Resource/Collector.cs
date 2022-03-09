using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using FixedMath;


public enum CollectorState
{
    notWorking = 0,
    idle = 1,
    goToResource = 2,
    working = 3,
    backToStop = 4,


}

public struct Collector  : IComponentData
{

    public CollectorState collectorState;
    public ResourceComponent resource;


    public int currentResourceStore;
}
