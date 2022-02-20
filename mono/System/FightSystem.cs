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

    private ResponseCommandSystem responseCommandSystem;
    private EntityManager entityManager;
    public event EventHandler sceneChangedComplete;
    public void InitFightScene()
    {
        var s = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<SelectionSystem>();
        s.GetMainCamera();
    }
 
    public override void InitInstance()
    {
        responseCommandSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<ResponseCommandSystem>();
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }
    public void ResponseFightStart() => ResourceService.Instance.canTransition = true;



    public void ResponseFightOp(PbMessage mes)
    {
        responseCommandSystem.ResponseFightOp(mes);
       



    }

 

 


}
