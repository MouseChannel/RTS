using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pb;
using System;
using Unity.Mathematics;
using UnityEngine.UI;
using Unity.Entities;

public   class ResponseNetSystem : ServiceSystem
// Singleton<ResponseNetSystem>
{

    // private ResponseCommandSystem responseCommandSystem;

    public List<WorkSystem> workList = new List<WorkSystem>();
    public List<Entity> allMovedUnit = new List<Entity>();
    public List<Entity> allObstacle = new List<Entity>();
    public List<ViewUnit> allGameobject = new List<ViewUnit>();

    int frameId = 0;


    public void InitFightScene()
    {



        var fow = World.GetOrCreateSystem<FOWSystem>();
        fow.InitFOW();
        var kDTreeSystem = World.GetOrCreateSystem<KDTreeSystem>();

    }

    protected override void OnCreate()
    {
        // responseCommandSystem = World.GetOrCreateSystem<ResponseCommandSystem>();


        workList.Add(World.GetOrCreateSystem<CommandSystem>());
        workList.Add(World.GetOrCreateSystem<CollectorSystem>());
        workList.Add(World.GetOrCreateSystem<PathFindSystem>());
        workList.Add(World.GetOrCreateSystem<KeepWalkingSystem>());
        workList.Add(World.GetOrCreateSystem<KDTreeSystem>());
        workList.Add(World.GetOrCreateSystem<AgentSystem>());


    }
    public void ResponseFightStart()
    {
        ResourceService.Instance.canTransition = true;

    }



    public void ResponseMoveCommand(FightMessage mes)
    {



        foreach (var i in mes.SelectedUnit)
        {
            var entity = allMovedUnit[i];
            Debug.Log(entity);
            // ChangeInhabitantState(entity, InhabitantState.Idle);


            if (GridSystem.Instance.GetGridArray()[mes.EndPos].isWalkable)
              EntityManager.AddComponentData(entity, new HasCommand { type = CommandType.move, commandData = mes.EndPos});

                // EntityManager.AddComponentData(entity, new PathFindParam { endPosition = mes.EndPos });
        }

    }
    public void ResponseInteractCommand(FightMessage mes)
    {

        // var interactEntity = allObstacle[mes.InteractObject];

        // var resourceComponent = EntityManager.GetComponentData<ResourceComponent>(interactEntity);

        foreach (var i in mes.SelectedUnit)
        {
            var entity = allMovedUnit[i];
            // ChangeInhabitantState(entity, InhabitantState.Collect);
            

              EntityManager.AddComponentData(allMovedUnit[i], new HasCommand { type = CommandType.collect, commandData = mes.InteractObject  });

            // EntityManager.AddComponentData(allMovedUnit[i], new CollectCommand {  resourceNo = mes.InteractObject, resource = resourceComponent });
        }

    }
    public void ResponseFightCommand(FightMessage mes)
    {
        var fightEntity = allMovedUnit[mes.EnemyUnit];



        foreach (var i in mes.SelectedUnit)
        {
            Debug.Log("fight");
            EntityManager.AddComponentData(allMovedUnit[i], new HasCommand { type = CommandType.fight, commandData = mes.EnemyUnit });
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
                case FightMessage.Types.BattleCMD.Fight:
                    ResponseFightCommand(i);
                    break;

            }
        }
        foreach (var i in workList)
        {
            i.Work();
        }



    }

    protected override void OnUpdate()
    {

    }
}
