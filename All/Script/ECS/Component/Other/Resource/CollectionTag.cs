using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;


public enum CollectorState{
    Idle = 1,
    GoToResource = 2,
    Working = 3,
    BackToStation = 4

}

public struct Collector  : IComponentData
{
    public CollectorState collectorState;
    public int resourceId;
}
