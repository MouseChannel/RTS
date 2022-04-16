using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;


unsafe public struct ExposedTransformSystemState : ISystemStateComponentData
{
    public Entity* exposedEntities;
    public int count;
}
