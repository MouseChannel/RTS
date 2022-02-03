using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pb;
public class LoginSystem : Singleton<LoginSystem>{
    public LoginWindow _loginWindow;
    public void Init(){
        
    }

    public void EnterLogin(){
        //登录窗口
        _loginWindow.SetWindowState();

    }

    public void ResponseLogin(PbMessage message){
        Debug.Log(NetService.Instance.Sid+ "Success login");
        
        GameRoot.Instance.tipWindow.AddTips("登陆成功");
        
        LobbySystem.Instance.EnterLobby();

    }
}
