using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pb;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SelectWindow : WindowRoot
{



    [SerializeField]
    [Header("生成prefab的位置")]
    private Transform playerPanel;
    [SerializeField]
    private Transform factionPanel;


    private Transform readyButton, unReadyButton;
    private int currentFactionIndex;

    private GameObject hideIconImage;
    public Transform test;

    void FindAllElement()
    {
        readyButton = transform.Find("SelectPanel/ReadyButton");
        unReadyButton = transform.Find("SelectPanel/UnReadyButton");
        hideIconImage = transform.Find("SelectPanel/Factions/HideIconImage").gameObject;
    }
    public override void InitWindow()
    {
        base.InitWindow();

        FindAllElement();

        //生成玩家卡片
        SpawnChild(playerPanel, GameRoot.Instance.roomCount, "UI/UIPrefab/SelectPlayerCard");


        //生成fation选择图标

        SpawnChild(factionPanel, GameRoot.Instance.factionCount, "UI/UIPrefab/FactionIcon");
        LoopChildAction(factionPanel, (Transform t, int index) =>
        {
            SetSprite(GetImage(t.Find("Mask/Icon")), "UI/UISprite/faction_" + index.ToString());
            OnClick(t.gameObject, ClickFactionIcon, index);
        });







        OnClick(readyButton.gameObject, ClickReadyButton);
        OnClick(unReadyButton.gameObject, ClickUnReadyButton);
        Debug.Log("selectwindow Init");
    }




    /// <summary>
    /// 更新玩家卡片
    /// </summary>
    /// <param name="message"></param>
    public void UpdateSelectData(PbMessage message)
    {
        string playerName = message.Name;
        int faction = message.SelectData.Faction;
        bool ready = message.SelectData.IsReady;
        string chatMes = message.SelectData.ChatMes;
        int index = message.RoomIndex;
        Debug.Log("Chat mes ;  " + message.SelectData.ChatMes);
        var player = playerPanel.GetChild(index);

        //用户头像还不会写
        // SetSprite(GetImage(PlayerPanel.GetChild(index)),)


        SetText(player.Find("Left/Name"), playerName);
        Image factionIcon = GetImage(player.Find("Right/faction/IconBG/factionIcon"));
        SetSprite(factionIcon, "UI/UISprite/faction_" + faction.ToString());

        if (ready)
        {
            SetActive(player.Find("Right/Ready/ReadyText"));
            SetActive(player.Find("Right/Ready/UnReadyText"), false);
            // SetMaterial(GetImage(player),"UI/UIMaterial/ReadyCard");



        }
        else
        {
            SetActive(player.Find("Right/Ready/ReadyText"), false);
            SetActive(player.Find("Right/Ready/UnReadyText"));
            ResetMaterial(GetImage(player));
        }



    }


    /// <summary>
    /// 更新当前选择的factionIcon
    /// </summary>
    /// <param name="alreadyReady"> 点击ready按钮后，所有icon失效</param>
    private void UpdateFactionIcon(bool alreadyReady = false)
    {
        if (alreadyReady)
        {

            hideIconImage.SetActive(true);
            for (int i = 0; i < GameRoot.Instance.factionCount; i++)
            {
                GetButton(factionPanel.GetChild(i)).interactable = false;

            }
        }
        else
        {
            hideIconImage.SetActive(false);
            LoopChildAction(factionPanel, (Transform t, int index) =>
            {
                GetButton(t).interactable = (index != currentFactionIndex);
                SetActive(t.Find("Active"), (index == currentFactionIndex));

            });

        }

    }

    public void ClickFactionIcon(PointerEventData ped, object[] args)
    {
        currentFactionIndex = (int)args[0];
        Debug.Log(currentFactionIndex);

        UpdateFactionIcon();


        //点击faction图标后，使之不可再 interactable
        LoopChildAction(factionPanel, (Transform t, int index) =>
        {
            GetButton(t).interactable = (index != currentFactionIndex);
            SetActive(t.Find("Active"), (index == currentFactionIndex));
        });

        PbMessage message = new PbMessage
        {
            Name = NetService.Instance.Sid.ToString(),
            Cmd = PbMessage.Types.CMD.Room,
            CmdRoom = PbMessage.Types.CmdRoom.SelectDate,

            SelectData = new SelectData
            {
                // PlayerName = _netService.Sid.ToString(),
                IsReady = false,
                Faction = currentFactionIndex,
            }
        };
        NetService.Instance.SendMessage(message);

    }
    public void ClickReadyButton(PointerEventData ped, object[] args)
    {

        SetActive(unReadyButton);
        SetActive(readyButton, false);
        PbMessage message = new PbMessage
        {
            Cmd = PbMessage.Types.CMD.Room,
            CmdRoom = PbMessage.Types.CmdRoom.SelectDate,

            SelectData = new SelectData
            {
                IsReady = true,
                Faction = currentFactionIndex,
            },


        };
        NetService.Instance.SendMessage(message);
        UpdateFactionIcon(true);



    }
    public void ClickUnReadyButton(PointerEventData ped, object[] args)
    {
        SetActive(readyButton);
        SetActive(unReadyButton, false);



        PbMessage message = new PbMessage
        {
            Cmd = PbMessage.Types.CMD.Room,
            CmdRoom = PbMessage.Types.CmdRoom.SelectDate,


            SelectData = new SelectData
            {
                IsReady = false,
                Faction = currentFactionIndex,
            },

        };
        NetService.Instance.SendMessage(message);
        UpdateFactionIcon();
    }

}
