using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KCPNET;
using Pb;
using System;


public class ClientSession : Session
{
    protected override void OnConnected() {
        
        }

    protected override void OnDisConnected() {
        }

    protected override void OnReceiveMessage(PbMessage pbMessage) {
            Debug.Log(pbMessage);
            GameRoot.Instance._netService.AddMessageQueue(pbMessage);
        }

    protected override void OnUpdate(DateTime now) {
        }
    
    
    
   
}
