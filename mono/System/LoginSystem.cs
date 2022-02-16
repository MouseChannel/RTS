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
       
        
     
        _loginWindow.SetWindowState(false);
        

        LobbySystem.Instance.EnterLobbyWindow();

    }
}
