using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRoot : Singleton<GameRoot>
{
    public TipWindow tipWindow;
    public int roomCount = 3,factionCount = 3;
    void Start()
    {
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    [HideInInspector]
    public NetService _netService;
    [HideInInspector]
    public ResourceService _resourceService;
    [HideInInspector]
    public AudioService _audioService;


    public void GetInstance() {

        _netService = NetService.Instance;
        _resourceService = ResourceService.Instance;
        _audioService = AudioService.Instance;
    }
    void Init(){
        GetInstance();
        _netService = GetComponent<NetService>();
        _netService.Init();
        // _resourceService = GetComponent<ResourceService>();
        // _resourceService.Init();
        // _audioService = GetComponent<AudioService>();
        // _audioService.Init();

        LoginSystem _loginSystem = GetComponent<LoginSystem>();
        _loginSystem.Init();    
        
        BattleSystem _battleSystem = GetComponent<BattleSystem>();
        _battleSystem.Init();
        
        // LobbySystem _lobbySystem = GetComponent<LobbySystem>();
        // _lobbySystem.Init();   

        // //login
        // _loginSystem.EnterLogin(); 
    }

    public void ShowTips(string tips) {
        // tipWindow.AddTips(tips);
    }



}
