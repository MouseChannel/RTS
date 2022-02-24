using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pb;
public class LoginSystem : Singleton<LoginSystem>
{
    private LoginWindow loginWindow;


    public override void InitInstance()
    {
        
    }
    public void EnterLogin()
    {
        Debug.Log("enter log");
        //登录窗口
        ResourceService.Instance.LoadMainWindow<LoginWindow>("UI/UIMainWindow/LoginWindow", ref loginWindow);

    }

    public void ResponseLogin(PbMessage message)
    {

        LobbySystem.Instance.EnterLobbyWindow();
    }

 


}
