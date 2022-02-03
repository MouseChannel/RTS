using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pb;

public class RoomSystem : Singleton<RoomSystem>
{
   public SelectWindow selectWindow;
   public ConfirmWindow confirmWindow;
    public LoadWindow loadWindow;
    public int index =-1;

   public void EnterSelectWindow(){
       LobbySystem.Instance.lobbyWindow.SetWindowState(false);
       confirmWindow.SetWindowState(false);
       selectWindow.SetWindowState();
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
        confirmWindow.SetWindowState();
        index = message.Index;

    }
    private void ResponseRoomDismiss(PbMessage message){
        confirmWindow.SetWindowState(false);
    }    
    private void ResponseSelect(PbMessage message){
        Debug.Log("RoomSelect start");
        EnterSelectWindow();



    }
    
    private void ResponseSelectData(PbMessage message){
        selectWindow.UpdateSelectData(message);
    }
    private void ResponseLoad(PbMessage message){
        selectWindow.SetWindowState(false);
        loadWindow.SetWindowState(true);

    }
    private void ResponseLoadData(PbMessage message){
        loadWindow.UpdateLoadPercent(message);
    }



}
