using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameRoot : SingletonMonoBehaviour<GameRoot>
{
    public TipWindow tipWindow;
    public int roomCount = 3,factionCount = 3;
    public event EventHandler  updateEvent;
    void Start()
    {
        Init();
        DontDestroyOnLoad(this);
    }

    // Update is called once per frame
    void Update()
    {
        updateEvent?.Invoke(this, EventArgs.Empty);
    }
    


 
    void Init(){
        // gameObject.AddComponent<NetService>();
        
        NetService.Instance.Init();
        ResourceService.Instance.Init();
        AudioService.Instance.Init();

        LoginSystem.Instance.Init();
        LobbySystem.Instance.Init();
        BattleSystem.Instance.Init();
 

        //login
        LoginSystem.Instance.EnterLogin();
        
    }
 
    public void ShowTips(string tips) {
        // tipWindow.AddTips(tips);
    }



}
