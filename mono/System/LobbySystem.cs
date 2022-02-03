using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pb;

public class LobbySystem : Singleton<LobbySystem>{
    public LobbyWindow lobbyWindow;
    
    public uint roomId;
   public  void Init(){
       

   }

   public void EnterLobby(){
        //登录窗口
        LoginSystem.Instance._loginWindow.SetWindowState(false);
        lobbyWindow.SetWindowState();
   }
   public void ResponseMatch(PbMessage message){
        switch (message.CmdMatch){
                    case PbMessage.Types.CmdMatch.JoinMatch:
                        ResponseJoinMatch(message);
                        break;
                    case PbMessage.Types.CmdMatch.QuitMatch:
                        ResponseQuitMatch(message);
                        break;
                    // case PbMessage.Types.CmdMatch.Dissmiss:
                    //     ResponseDismiss(message);
                    //     break;
                }
   }
    private void ResponseJoinMatch(PbMessage message){
        Debug.Log("    Success JOIN mATCH");
        lobbyWindow.ResponseJoinMatch();      
       

    }
    private void ResponseQuitMatch(PbMessage message){
        Debug.Log("    Success quit mATCH");
        
        lobbyWindow.ResponseQuitMatch();     
       

    }
    // public void ResponseConfirm(PbMessage message){
    //     Debug.Log("I Need Confirm");
    //     roomId = message.RoomId;
    //     confirmWindow.SetWindowState();

    // }
    // private void ResponseDismiss(PbMessage message){
    //     Debug.Log("I Need Confirm");
    //     confirmWindow.SetWindowState(false);
    //     roomId = 1<<32;
        

    // }
}
