using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using System;

unsafe public struct ExposedTransformSystemState : ISystemStateComponentData
{
    public UIntPtr exposedEntities;
    public Entity pointerType;
    public int count;
    public Entity GetEntity(int i)
    {
        return *((Entity*)exposedEntities.ToPointer() + i);
    }

}
