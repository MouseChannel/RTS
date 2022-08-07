using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Entities;

public class GameRoot : SingletonMonoBehaviour<GameRoot>
{
    public TipWindow tipWindow;
    public int roomCount = 3,factionCount = 3;
 
 
  
    void Start()
    {
        InitUI();
        DontDestroyOnLoad(this);
    }

    // Update is called once per frame
    void Update()
    {
        NetService.Instance.Update();
        
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
 
