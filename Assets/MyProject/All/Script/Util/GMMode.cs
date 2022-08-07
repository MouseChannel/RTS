using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pb;
public class GMMode : MonoBehaviour
{

    public void StartGMMode(){
        NetService.Instance.SendMessage(PbTool.Instance.MakeTest());
    }
}
