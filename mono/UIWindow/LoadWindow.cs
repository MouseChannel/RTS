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
    public List<PlayerData> playerDatas  ;
       
    private List<TMP_Text> percentTextList;
    [SerializeField]private Transform playerCardTransform;
    public override void InitWindow(){
      base.InitWindow();

   }
    public void InitLoadPlayerData(PbMessage message){
        playerDatas = new List<PlayerData>();
        var newPlayerDatas = message.RoomSelectData;
        foreach(var i in newPlayerDatas){
            GameObject playerCard = ResourceService.Instance.LoadPrefab("UI/UIPrefab/LoadPlayerCard");
            playerCard.transform.SetParent(playerCardTransform,false);
            playerDatas.Add(new PlayerData{
                name = i.PlayerName,
                faction = i.Faction
            });

        }
        


    }
 

    public void UpdateLoadPercent(PbMessage message ){
        var percent = message.LoadPercent;
   
        
    }





}
