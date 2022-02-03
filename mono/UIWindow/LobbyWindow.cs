using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pb;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class LobbyWindow : WindowRoot
{
   [SerializeField]
   private GameObject unMatch, match;
   protected override void InitWindow(){
      base.InitWindow();
      OnClick(match,ClickJoinMatch);
      OnClick(unMatch,ClickQuitMatch);
   }
   public void ClickJoinMatch(PointerEventData ped ,object[] args){
      PbMessage message = new PbMessage{
         Cmd =  PbMessage.Types.CMD.Match,
         CmdMatch = PbMessage.Types.CmdMatch.JoinMatch,
         // SId = _netService.Sid
      };
      NetService.Instance.SendMessage(message);

   }
   public void ClickQuitMatch(PointerEventData ped ,object[] args){
      
       PbMessage message = new PbMessage{
         Cmd =  PbMessage.Types.CMD.Match,
         CmdMatch = PbMessage.Types.CmdMatch.QuitMatch,
         // SId = _netService.Sid
      };
      NetService.Instance.SendMessage(message);

   }

   public void ResponseJoinMatch(){
      Debug.Log("Setbuttomn");
      unMatch.SetActive(true);
      match.SetActive(false);
   }
   public void ResponseQuitMatch(){
      unMatch.SetActive(false);
      match.SetActive(true);
   }
   
}
