using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
 
public struct CanBeSelected : IComponentData
{
}
public struct SelectableEntityTag : IComponentData
{
}

public struct SelectionColliderTag : IComponentData
{
}
