using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;


public enum FighterState{
    
    idle = 0,
    chaseEnemy = 1,
    fight = 2

}

public struct Fighter : IComponentData
{
    public FighterState state;
    public int beforeEnemyPositionIndex;
}
