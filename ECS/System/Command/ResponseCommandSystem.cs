using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using RVO;
using Unity.Collections;
using Pb;

public class ResponseCommandSystem : SystemBase
{
  public List<WorkSystem> workList = new List<WorkSystem>();
    public NativeList<Entity> allMovedUnit = new NativeList<Entity>(Allocator.Persistent);
    protected override void OnUpdate()
    {
 


    }
     public void ResponseMoveCommand(FightMessage mes)
    {
        Debug.Log("response move");
        foreach (var i in mes.SelectedUnit){
            EntityManager.AddComponentData(allMovedUnit[i], new PathFindParams { endPosition = new Unity.Mathematics.int2(((byte)mes.EndPos[0]), mes.EndPos[1]) });
        }
 
    }
    public void ResponseFightOp(PbMessage mes)
    {
        foreach (var i in workList)
        {
            i.Work();
        }

        foreach (var i in mes.FightMessage)
        {
            switch (i.BattleCMD)
            {
                case FightMessage.Types.BattleCMD.Move:
                    ResponseMoveCommand(i);
                    break;

            }
        }

    }
    protected override void OnDestroy()
    {
        allMovedUnit.Dispose();
    }
}
