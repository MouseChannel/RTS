using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;


public enum FighterState{
    notFightng = 0,
    idle = 1,
    chaseEnemy = 2,
    fight = 3

}

public struct Fighter : IComponentData
{
    public FighterState state;
    // public int
}
