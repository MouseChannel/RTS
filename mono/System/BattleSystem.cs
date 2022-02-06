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
   void Start(){
      responseCommandSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<ResponseCommandSystem>();
   }
   
   public  void Init(){
        
      
   }
 
   bool hasCommand  = false;
 
   int frame = 0;
   bool startFramecount = false;
   public void ResponseBattle(PbMessage mes){
      switch(mes.BattleCMD){
         case PbMessage.Types.BattleCMD.Move:
            responseCommandSystem.needResponse = true;
            foreach(var i in mes.SelectedUnit){
               responseCommandSystem.needMovedUnit.Add(i);
            }
            

            break;
      }

   //   endx = mes.C;
   //   endy = mes.D;
     hasCommand = true;
     startFramecount = true;
      
    

   }
   private void FixedUpdate() {

      
   }

   
}
