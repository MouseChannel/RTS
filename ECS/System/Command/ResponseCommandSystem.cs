using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using RVO;
using Unity.Collections;
// [AlwaysUpdateSystem]
public class ResponseCommandSystem : SystemBase
{
    public bool needResponse;
    public NativeList<Entity> needMovedUnit = new NativeList<Entity>(Allocator.Persistent);
    protected override void OnUpdate()
    {
        // if(!needResponse) return;
        // needResponse = false;

        // Entities.ForEach((Entity entity, in Agent agent)=>{

            
        // }).WithoutBurst().Run();

        // needMovedUnit.Clear();
        
        
    }
    protected override void OnDestroy()
    {
        needMovedUnit.Dispose();
    }
}
