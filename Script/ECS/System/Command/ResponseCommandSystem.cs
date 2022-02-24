using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using RVO;
using Unity.Collections;
using Pb;
using UnityEngine.UI;
 [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
 [DisableAutoCreation]
public class ResponseCommandSystem : SystemBase
{
  public List<WorkSystem> workList = new List<WorkSystem>();
    public  List<Entity> allMovedUnit = new  List<Entity>( );
    private Transform  frame;
    private Text id;
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
        if(frame == null){
            ResourceService.Instance.LoadSubWindow<Transform>("UI/UISubWindow/Image", -300, 125, ref frame);
            id = frame.Find("Text").GetComponent<Text>();
        }
        


        foreach (var i in mes.FightMessage)
        {
            switch (i.BattleCMD)
            {
                case FightMessage.Types.BattleCMD.Move:
                    id.text = mes.FrameId.ToString();
                    ResponseMoveCommand(i);      
                    break;
            }
        }
        foreach (var i in workList)
        {
            i.Work();
        }

    }
 
}
