using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using FixedMath;


// public enum DoingTaskState
// {

//     idle = 0,
//     goToDestination = 1,
//     working = 2,
//     backToStop = 3,


// }

public struct Collector : IComponentData
{

    public DoingTaskState state;
    public int currentResourceStore;
    
}
