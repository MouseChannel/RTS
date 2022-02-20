using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Pb;
using Unity.Mathematics;
using Google.Protobuf.Collections;
public class PbTool : Singleton<PbTool>
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
    public PbMessage MakeBattleStart()
    {

        return new PbMessage
        {

            Cmd = PbMessage.Types.CMD.Room,
            CmdRoom = PbMessage.Types.CmdRoom.FightStart
        };
    }

    public static PbMessage MakeMove(float3 destinationPox, List<int> seletedUnits)
    {
        GridInit.Instance.pathfindingGrid.GetXZ(destinationPox, out int endx, out int endy);
        var sendFightMessage = new FightMessage
        {
            BattleCMD = FightMessage.Types.BattleCMD.Move,
            SelectedUnit = { seletedUnits },
            EndPos = { endx, endy }
        };


        return new PbMessage
        {
            Cmd = PbMessage.Types.CMD.Room,
            CmdRoom = PbMessage.Types.CmdRoom.FightOp,
            SendFightMessage = sendFightMessage
        };
    }


}
