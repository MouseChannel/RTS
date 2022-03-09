using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pb;
using System.Threading;
using Unity.Entities;

public class LobbySystem : Singleton<LobbySystem>
{
    private LobbyWindow lobbyWindow;
    private ConfirmWindow confirmWindow;
    private SelectWindow selectWindow;
    private LoadWindow loadWindow;
   

    public override void InitInstance()
    {

        
    }

    public void EnterLobbyWindow()
    {
        Debug.Log("enter lobby window");
        //登录窗口
        // Resources.Load<LobbyWindow>("UI/UIMainWindow/LobbyWindow");
        ResourceService.Instance.LoadMainWindow<LobbyWindow>("UI/UIMainWindow/LobbyWindow", ref lobbyWindow);

        Debug.Log("loadLobbt");
        // lobbyWindow.SetWindowState();
    }
    public void EnterConfirmWindow()
    {
        Debug.Log("EnterConfirm");
        ResourceService.Instance.LoadSubWindow<ConfirmWindow>("UI/UISubWindow/ConfirmWindow", 0, 0, ref confirmWindow);

    }
    public void EnterSelectWindow()
    {
        //    LobbySystem.Instance.lobbyWindow.SetWindowState(false);
        ResourceService.Instance.LoadMainWindow<SelectWindow>("UI/UIMainWindow/SelectWindow", ref selectWindow);


    }
    int lastPercent = 0;
    public void EnterLoadWindow()
    {
        ResourceService.Instance.LoadMainWindow<LoadWindow>("UI/UIMainWindow/LoadWindow", ref loadWindow);
        ResourceService.Instance.AsyncLoadScene("FightScene", SceneLoadProgress, SceneLoadDone, sceneChangedComplete);

        void SceneLoadProgress(float val)
        {
            int percent = (int)(val * 100);
            if (lastPercent != percent)
            {
                var mes = PbTool.Instance.MakeLoadProgress(percent);
                NetService.Instance.SendMessage(mes);
                lastPercent = percent;
            }
        }

        void SceneLoadDone()
        {
            var mes = PbTool.Instance.MakeBattleStart();


            NetService.Instance.SendMessage(mes);


        }
        void sceneChangedComplete()
        {
            ResponseNetSystem.Instance.InitFightScene();
        }
    }
    #region match
    public void ResponseMatch(PbMessage message)
    {
        switch (message.CmdMatch)
        {
            case PbMessage.Types.CmdMatch.JoinMatch:
                ResponseJoinMatch(message);
                break;
            case PbMessage.Types.CmdMatch.QuitMatch:
                ResponseQuitMatch(message);
                break;

        }
    }
    private void ResponseJoinMatch(PbMessage message)
    {

        lobbyWindow.ResponseJoinMatch();


    }
    private void ResponseQuitMatch(PbMessage message)
    {


        lobbyWindow.ResponseQuitMatch();


    }

    #endregion
    public void ResponseRoom(PbMessage message)
    {
        switch (message.CmdRoom)
        {
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
            case PbMessage.Types.CmdRoom.FightStart:
                ResponseNetSystem.Instance.ResponseFightStart();
                break;


        }


    }
    private void ResponseConfirm(PbMessage message)
    {
        // confirmWindow.SetWindowState();
        Debug.Log("Resd  condi");
        EnterConfirmWindow();


    }
    private void ResponseRoomDismiss(PbMessage message)
    {
        Debug.Log("room dismiss");
        if (confirmWindow) ResourceService.Instance.Destroy(confirmWindow.gameObject);






        if (selectWindow) ResourceService.Instance.Destroy(selectWindow.gameObject);
    }
    private void ResponseSelect(PbMessage message)
    {
        Debug.Log("RoomSelect start");
        EnterSelectWindow();



    }

    private void ResponseSelectData(PbMessage message)
    {
        selectWindow.UpdateSelectData(message);
    }
    private void ResponseLoad(PbMessage message)
    {
        // selectWindow.SetWindowState(false);
        // loadWindow.SetWindowState(true);
        EnterLoadWindow();
        loadWindow.InitLoadPlayerData(message);

    }
    private void ResponseLoadData(PbMessage message)
    {
        loadWindow.UpdateLoadPercent(message);
    }







}
