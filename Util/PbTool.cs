using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Pb;
using Unity.Mathematics;
using Google.Protobuf.Collections;
public class PbTool :Singleton<PbTool>
{
 

    public PbMessage MakeLoadProgress(int percent)
    {
   
        return new PbMessage
        {
            Cmd = PbMessage.Types.CMD.Room,
            CmdRoom = PbMessage.Types.CmdRoom.LoadData,
            LoadPercent = percent
        };
    }
    public PbMessage MakeBattleStart(){
    
        return new PbMessage
        {
            
            Cmd = PbMessage.Types.CMD.Room,
            CmdRoom = PbMessage.Types.CmdRoom.FightStart
        };
    }

    public static PbMessage MakeMove(int2 endPox, List<int> seletedUnits)
    {


        return new PbMessage
        {
            // Cmd = PbMessage.Types.CMD.Battle,
            // BattleCMD = PbMessage.Types.BattleCMD.Move,
            // SelectedUnit = {seletedUnits},
            // EndPos = {endPox.x, endPox.y}


        };
    }


}
