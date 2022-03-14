using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using System;



public enum DoingTaskState
{

    idle = 0,
    goToDestination = 1,
    working = 2,
    goToSecondDestination = 3


}

public struct Fighter : IComponentData
{
    public DoingTaskState state;
    public int beforeEnemyPositionIndex;
}
