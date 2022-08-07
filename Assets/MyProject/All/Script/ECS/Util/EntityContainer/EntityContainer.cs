using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;

public static class EntityContainer 
{
    public static NativeList<Entity> allUnitEntity = new NativeList<Entity>(Allocator.Persistent);

    public static int AddEntity(Entity entity){
        allUnitEntity.Add(entity);
        return allUnitEntity.Length;
    }
    public static void RemoveEntity(int index){
        allUnitEntity[index] = Entity.Null;
    } 
}
