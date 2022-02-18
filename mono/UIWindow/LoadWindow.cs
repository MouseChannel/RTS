using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pb;
using TMPro;


public class LoadWindow : WindowRoot
{
    public class PlayerData{
        public string name;
        public int faction;
    }
    public List<PlayerData> playerDatas;
       
    private List<Text> percentTextList;
    private List<Image> percentBarList;
    private Transform playerCardTransform;
    public override void InitWindow(){
      base.InitWindow();
        playerCardTransform = transform.Find("PlayerCards");

    }
    public void InitLoadPlayerData(PbMessage message){
        playerDatas = new List<PlayerData>();
        percentTextList = new List<Text>();
        percentBarList = new List<Image>();
        var newPlayerDatas = message.RoomSelectData;
        foreach(var i in newPlayerDatas){
            Debug.LogError(i);
            playerDatas.Add(new PlayerData{
                name = i.PlayerName,
                faction = i.Faction
            });
            GameObject playerCard = ResourceService.Instance.LoadPrefab("UI/UIPrefab/LoadPlayerCard");
            SetParent(playerCard, playerCardTransform);
            var percentBar = GetImage(playerCard.transform, "Header/LoadingBar/FillBg/Fill");
            percentBarList.Add(percentBar);

            var percentText = GetText(playerCard.transform, "Header/LoadingBar/LoadPercent");
            percentTextList.Add(percentText);

        }
        


    }
 
 

    public void UpdateLoadPercent(PbMessage message ){
        var index = message.RoomIndex;
        var percent = message.LoadPercent;
        Debug.Log("index "+  index);
        
        percentBarList[index].fillAmount =  percent / 100f;
        percentTextList[index].text = percent.ToString() + "%";
 
        
   
   
        
    }





}
