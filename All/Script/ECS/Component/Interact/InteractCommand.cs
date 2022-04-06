using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public enum CommandType{
    move = 0,
    collect = 1,
    fight = 2,
}

public struct HasCommandState : ISystemStateComponentData{

    public int commandData;
    public CommandType type;
}


public struct DoingCollect : IComponentData{
    public int resourceNo;

}

public struct DoingFight : IComponentData{
    public int enemyNo;
}


