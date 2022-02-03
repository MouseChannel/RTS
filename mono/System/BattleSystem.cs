using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pb;
using System;
using Unity.Mathematics;
using UnityEngine.UI;
public class BattleSystem : Singleton<BattleSystem>
{
   public Text frameText;
   
   public  void Init(){
        

   }
   // public Action<int2 , int2 > moveParams;
   public class MoveParams{
      public  int2 startPos;
      public int2 endPos;
   }
   public delegate void Move(int2 startPos, int2 endPos);
   
   public event Move move;
   bool hasCommand  = false;
   int endx = 0,endy = 0;
   int frame = 0;
   bool startFramecount = false;
   public void ResponseBattle(PbMessage mes){

     endx = mes.C;
     endy = mes.D;
     hasCommand = true;
     startFramecount = true;
      
      // move?.Invoke(new int2(mes.A,mes.B), new int2(mes.C, mes.D));

   }
   private void FixedUpdate() {
      if(startFramecount)
         frame ++;
      if(hasCommand){
         move?.Invoke(new int2(0,0), new int2(endx,endy));
         hasCommand =false;
         frameText.text = frame.ToString();
      }

      
   }

   
}
