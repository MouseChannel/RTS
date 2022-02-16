using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pb;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ConfirmWindow : WindowRoot
{
   [SerializeField]
   private GameObject confirmButton;
    [SerializeField]
   private Text confirmTest;
    [SerializeField]
   private GameObject confirmEffect;
   public override void InitWindow(){
      base.InitWindow();
      OnClick(confirmButton,ClickConfirm);
   }
   public void ClickConfirm(PointerEventData ped ,object[] args){
       PbMessage message = new PbMessage{

         Cmd =  PbMessage.Types.CMD.Room,
         CmdRoom = PbMessage.Types.CmdRoom.Confirm

         
      };
      NetService.Instance.SendMessage(message);
      confirmTest.text = "已接受";
      confirmEffect.SetActive(false);
      GetButton(confirmButton.transform).interactable = false;
   }


}
