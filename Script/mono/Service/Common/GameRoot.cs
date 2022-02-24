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
        InitUI();
        DontDestroyOnLoad(this);
    }

    // Update is called once per frame
    void Update()
    {
        NetService.Instance.Update();
        // updateEvent?.Invoke(this, EventArgs.Empty);
    }
 
    public void MyStartCoroutine(IEnumerator action){
        StartCoroutine(action);
    }
 
    


 
    void InitUI(){
        // gameObject.AddComponent<NetService>();
        
        NetService.Instance.Init();
 
 

 
        //login
        LoginSystem.Instance.EnterLogin();
        
    }
    void InitfightScene(){
        
    }
 
    public void ShowTips(string tips) {
        // tipWindow.AddTips(tips);
    }



}
