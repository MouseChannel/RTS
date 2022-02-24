using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using FixedMath;
public struct AutoAttackBuilding : IComponentData
{
    public FixedInt attackRange;
    public int enemyUnitNo;
}
