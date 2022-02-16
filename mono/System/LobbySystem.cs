using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pb;

public class LobbySystem : Singleton<LobbySystem>{
    [HideInInspector]public LobbyWindow lobbyWindow;
    [HideInInspector]public ConfirmWindow confirmWindow;
    [HideInInspector]public SelectWindow selectWindow;
    [HideInInspector]public LoadWindow loadWindow;
     
   public  void Init(){
       

   }

    public void EnterLobbyWindow(){
        //登录窗口
        ResourceService.Instance.LoadMainWindow("UI/UIMainWindow/LobbyWindow",ref lobbyWindow);
     
        // lobbyWindow.SetWindowState();
   }
    public void EnterConfirmWindow(){
       Debug.Log("EnterConfirm");
       ResourceService.Instance.LoadSubWindow("UI/UISubWindow/ConfirmWindow",0, 0, ref confirmWindow);
 
   }
    public void EnterSelectWindow(){
    //    LobbySystem.Instance.lobbyWindow.SetWindowState(false);
       ResourceService.Instance.LoadMainWindow("UI/UIMainWindow/SelectWindow",ref selectWindow);

 
   }
   public void EnterLoadWindow(){
       ResourceService.Instance.LoadMainWindow("UI/UIMainWindow/LoadWindow", ref loadWindow);
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
    public void ResponseRoom(PbMessage message){
        switch (message.CmdRoom){
            case PbMessage.Types.CmdRoom.Confirm:
                ResponseConfirm(message);
                break;
            case PbMessage.Types.CmdRoom.Dismissed:
                ResponseRoomDismiss(message);
                break;
            case PbMessage.Types.CmdRoom.Select:
                ResponseSelect(message);
                break;
            case PbMessage.Types.CmdRoom.SelectDate:
                ResponseSelectData(message);
                break;
            case PbMessage.Types.CmdRoom.Load:
                ResponseLoad(message);
                break;
            case PbMessage.Types.CmdRoom.LoadData:
                ResponseLoadData(message);
                break;

                }
    

    }
    private void ResponseConfirm(PbMessage message){
        // confirmWindow.SetWindowState();
        Debug.Log("Resd  condi");
        EnterConfirmWindow();
       

    }
    private void ResponseRoomDismiss(PbMessage message){
      
    }    
    private void ResponseSelect(PbMessage message){
        Debug.Log("RoomSelect start");
        EnterSelectWindow();



    }
    
    private void ResponseSelectData(PbMessage message){
        selectWindow.UpdateSelectData(message);
    }
    private void ResponseLoad(PbMessage message){
        // selectWindow.SetWindowState(false);
        // loadWindow.SetWindowState(true);
        EnterLoadWindow();
        loadWindow.InitLoadPlayerData(message);

    }
    private void ResponseLoadData(PbMessage message){
        loadWindow.UpdateLoadPercent(message);
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
