using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pb;
using System;
using Unity.Mathematics;
using UnityEngine.UI;
using Unity.Entities;
 
public class ResponseNetSystem :  Singleton<ResponseNetSystem>
{

    // private ResponseCommandSystem responseCommandSystem;
    private EntityManager entityManager;
    public List<WorkSystem> workList = new List<WorkSystem>();
    public List<Entity> allMovedUnit = new List<Entity>();
    public List<Entity> allObstacle = new List<Entity>();
    public List<ViewUnit> allGameobject = new List<ViewUnit>();
    private Transform frame;
    int frameId = 0;
    private Text id;

    public void InitFightScene()
    {
        // var s = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<SelectionSystem>();
        // s.GetMainCamera();

        var fow = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<FOWSystem>();
        fow.InitFOW();
        var kDTreeSystem =  World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<KDTreeSystem>();
        // kDTreeSystem.UpdateObstacleTree();
    }

    public override void InitInstance()
    {
        // responseCommandSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<ResponseCommandSystem>();
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;


        workList.Add(World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<CollectorSystem>());
        workList.Add(World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<PathFindSystem>());
        workList.Add(World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<KeepWalkingSystem>());
        workList.Add(World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<KDTreeSystem>());
        workList.Add(World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<AgentSystem>());


    }
    public void ResponseFightStart()
    {
        ResourceService.Instance.canTransition = true;

    }



    public void ResponseMoveCommand(FightMessage mes)
    {


        foreach (var i in mes.SelectedUnit)
        {
            if (GridSystem.Instance.GetGridArray()[mes.EndPos].isWalkable)
                entityManager.AddComponentData(allMovedUnit[i], new PathFindCommand {   endPosition = mes.EndPos });
        }

    }
    public void ResponseInteractCommand(FightMessage mes)
    {
        var collectCommand = allObstacle[mes.InteractObject];

        var resourceComponent = entityManager.GetComponentData<ResourceComponent>(collectCommand);

        foreach (var i in mes.SelectedUnit)
        {
            Debug.Log("Add");
            entityManager.AddComponentData(allMovedUnit[i], new CollectCommand {  resource = resourceComponent });
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
                case FightMessage.Types.BattleCMD.Interact:
                    ResponseInteractCommand(i);
                    break;

            }
        }
        foreach (var i in workList)
        {
            i.Work();
        }



    }






}
