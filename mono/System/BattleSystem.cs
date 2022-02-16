using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pb;
using System;
using Unity.Mathematics;
using UnityEngine.UI;
using Unity.Entities;
public class BattleSystem : Singleton<BattleSystem>
{
   public Text frameText;
   private ResponseCommandSystem responseCommandSystem;
   private EntityManager entityManager;
   void Start(){
      responseCommandSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<ResponseCommandSystem>();
      entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
   }
   
   public  void Init(){
        
      
   }
 
   bool hasCommand  = false;
 
   int frame = 0;
   bool startFramecount = false;
   public void ResponseBattle(PbMessage mes){
      // switch(mes.FightMessage.){
      //    case FightMessage.Types.BattleCMD.Move: 
      //       foreach(var i in mes.SelectedUnit){
      //          var e = responseCommandSystem.needMovedUnit[i];
      //          entityManager.AddComponentData<PathFindParams>(e,new PathFindParams{endPosition = new int2(mes.EndPos[0], mes.EndPos[1])}); 
      //       }
      //       break;
      // }

   //   endx = mes.C;
   //   endy = mes.D;
     hasCommand = true;
     startFramecount = true;
      
    

   }
   private void FixedUpdate() {

      
   }

   
}
