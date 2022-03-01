using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pb;
using System;
using Unity.Mathematics;
using UnityEngine.UI;
using Unity.Entities;
using RVO;
public class FightSystem : Singleton<FightSystem>
{

    // private ResponseCommandSystem responseCommandSystem;
    private EntityManager entityManager;
    public List<WorkSystem> workList = new List<WorkSystem>();
    public  List<Entity> allMovedUnit = new  List<Entity>( );
    public List<ViewUnit> allGameobject = new List<ViewUnit>();
    private Transform  frame;
    int frameId = 0;
    private Text id;
   
    public void InitFightScene()
    {
        // var s = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<SelectionSystem>();
        // s.GetMainCamera();

        var fow = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<FOWSystem>();
        fow.InitFOW();
    }
 
    public override void InitInstance()
    {
        // responseCommandSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<ResponseCommandSystem>();
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        
        workList.Add(World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<PathFindSystem>());
        workList.Add(World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<HandoverSystem>());
        workList.Add(World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<KDTreeSystem>());
        workList.Add(World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<AgentSystem>());

 
    }
    public void ResponseFightStart() {
        ResourceService.Instance.canTransition = true;
      
    }
   


    public void ResponseMoveCommand(FightMessage mes)
    {


        foreach (var i in mes.SelectedUnit){
            entityManager.AddComponentData(allMovedUnit[i], new PathFindParams { endPosition = mes.EndPos });
        }
 
    }

    public void ResponseFightOp(PbMessage mes)
    {
        // responseCommandSystem.ResponseFightOp(mes);
        frameId++;
        // if(frame == null){
        //     ResourceService.Instance.LoadSubWindow<Transform>("UI/UISubWindow/Image", -300, 125, ref frame);
        //     id = frame.Find("Text").GetComponent<Text>();
        // }
        
        foreach (var i in mes.FightMessage)
        {
            switch (i.BattleCMD)
            {
                case FightMessage.Types.BattleCMD.Move:
                    // id.text = (1/Time.unscaledDeltaTime ).ToString();
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
