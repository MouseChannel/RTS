using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pb;
using Unity.Mathematics;
using Google.Protobuf.Collections;
public static class PbTool 
{
    // public static PbTool Instance{
    //     get {
    //         if(instance == null){
    //             instance = new PbTool();
    //         }
    //         return instance;
    //     }
    // }
    // private static PbTool instance;
    public static PbMessage MakeMove(int2 endPox, List<int> seletedUnits){


        return new PbMessage{
            Cmd = PbMessage.Types.CMD.Battle,
            BattleCMD = PbMessage.Types.BattleCMD.Move,
            SelectedUnit = {seletedUnits},
            


        };
    }

    
}
