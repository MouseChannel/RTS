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
   public override void InitWindow(){
      base.InitWindow();
      Debug.Log("Lobby listen ");
      OnClick(match,ClickJoinMatch);
      OnClick(unMatch,ClickQuitMatch);
   }
   public void ClickJoinMatch(PointerEventData ped ,object[] args){
      Debug.Log("join match");
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
      
      unMatch.SetActive(true);
      match.SetActive(false);
   }
   public void ResponseQuitMatch(){
      unMatch.SetActive(false);
      match.SetActive(true);
   }
   
}
