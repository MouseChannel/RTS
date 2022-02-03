using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pb;
using UnityEngine.EventSystems;
public class LoginWindow : WindowRoot
{
   [SerializeField]
   private GameObject loginButton;
   protected override void InitWindow(){
      base.InitWindow();
      OnClick(loginButton,ClickLogin);
   }
   public void ClickLogin(PointerEventData ped ,object[] args){
      // NetService.Instance.StartConnect();
      Debug.Log("send Login");
      
      PbMessage message = new PbMessage{
         Cmd =  PbMessage.Types.CMD.Login,
         // Cmd = PbMessage.Types.CMD.Login,
         Name = "login",
      };
      NetService.Instance.SendMessage(message);
      // NetService.Instance.SendMessage(message, (bool result) =>{
      //    if(result == false){
      //       NetService.Instance.Init();
      //    }
      // });
   }
}
