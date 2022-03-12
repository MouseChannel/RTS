using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using FixedMath;



public enum InhabitantState
{
    Idle = 0,
    Fight = 1,
    Collect = 2,
    Build = 3
}
public struct InhabitantComponent : IComponentData
{
    public InhabitantState state;
 
}
