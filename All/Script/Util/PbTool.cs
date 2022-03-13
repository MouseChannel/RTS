using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Pb;
using Unity.Mathematics;
using Google.Protobuf.Collections;
using Unity.Collections;

public class PbTool : Singleton<PbTool>
{
    public PbMessage MakeTest()
    {
        return new PbMessage
        {
            Cmd = PbMessage.Types.CMD.Chat,

        };
    }


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
        var endIndex = GridSystem.Instance.GetGridIndex(destinationPox);
        var sendFightMessage = new FightMessage
        {
            BattleCMD = FightMessage.Types.BattleCMD.Move,
            SelectedUnit = { seletedUnits },
            EndPos = endIndex
        };


        return new PbMessage
        {
            Cmd = PbMessage.Types.CMD.Room,
            CmdRoom = PbMessage.Types.CmdRoom.FightOp,
            SendFightMessage = sendFightMessage
        };
    }

    public static PbMessage MakeInteract(int entityNo, List<int> seletedUnits)
    {

        var sendFightMessage = new FightMessage
        {
            BattleCMD = FightMessage.Types.BattleCMD.Interact,
            SelectedUnit = { seletedUnits },
            InteractObject = entityNo
        };


        return new PbMessage
        {
            Cmd = PbMessage.Types.CMD.Room,
            CmdRoom = PbMessage.Types.CmdRoom.FightOp,
            SendFightMessage = sendFightMessage
        };
    }
    public static PbMessage MakeFight(int entityNo, List<int> seletedUnits)
    {
        var sendFightMessage = new FightMessage
        {
            BattleCMD = FightMessage.Types.BattleCMD.Fight,
            SelectedUnit = { seletedUnits },
            EnemyUnit = entityNo
        };


        return new PbMessage
        {
            Cmd = PbMessage.Types.CMD.Room,
            CmdRoom = PbMessage.Types.CmdRoom.FightOp,
            SendFightMessage = sendFightMessage
        };
    }

    public override void InitInstance()
    {

    }
}
