using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using RVO;
// [AlwaysUpdateSystem]
public class ResponseCommandSystem : SystemBase
{
    public bool needResponse;
    public List<int> needMovedUnit = new List<int>();
    protected override void OnUpdate()
    {
        if(!needResponse) return;
        needResponse = false;

        Entities.ForEach((Entity entity, in Agent agent)=>{
            if(needMovedUnit.Contains(agent.id_)){
                needMovedUnit.Remove(agent.id_);
                Debug.Log("work");
            }
            
        }).WithoutBurst().Run();

        needMovedUnit.Clear();
        
    }
}
