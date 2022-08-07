using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using FixedMath;



public enum InhabitantTaskType
{
    Idle = 0,
    Fight = 1,
    Collect = 2,
    Build = 3
}
public enum DoingTaskState
{

    idle = 0,
    goToDestination = 1,
    working = 2,
    goToSecondDestination = 3


}
public struct CollectorTag : IComponentData{
    //use firstValue as currentResourceStore
}
public struct FighterTag : IComponentData{
    //use firstValue as beforeEnemyPositionIndex
}
public struct InhabitantComponent : IComponentData
{
    public InhabitantTaskType taskType;
    public DoingTaskState taskState;
    public int objectNo;
    public int firstValue;
    public int secondValue;

}
