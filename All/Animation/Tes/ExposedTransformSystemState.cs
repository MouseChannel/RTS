using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using System;

unsafe public struct ExposedTransformSystemState : ISystemStateComponentData
{
    public UIntPtr exposedEntities;
    [HideInInspector]
    public Entity pointerType;
    public int count;
    public Entity GetEntity(int i)
    {
        return *((Entity*)exposedEntities.ToPointer() + i);
    }
    
    /// <summary>
    /// actually not release Memory, Just put back to pool
    /// </summary>
    public void Release()
    {
        AllocateNativeArrayPool.GiveBackToPool(pointerType, exposedEntities);
    }

}
