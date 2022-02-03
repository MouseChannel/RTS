using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pb;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SelectWindow : WindowRoot
{
   [SerializeField]
   [Header("Prefab：玩家卡片和factionIcon")]
   private GameObject playerCard;
    [SerializeField]
   private GameObject factionIcon;
   
   [SerializeField]
   [Header("生成prefab的位置")]
   private Transform playerPanel;
   [SerializeField]
   private Transform factionPanel;

  
   [SerializeField]
   private Transform readyButton, unReadyButton;
   private int currentFactionIndex;
   [SerializeField]
   private GameObject   hideIconImage;
   public Transform test;
   
   // void Start(){
   //    InitWindow();

   // }
   protected override void InitWindow(){
      base.InitWindow();
      //生成玩家卡片
      for(int i=0;i<GameRoot.Instance.roomCount;i++){
         GameObject go = Instantiate(playerCard);
         RectTransform rect = go.GetComponent<RectTransform>();
         rect.SetParent(playerPanel,false);
         rect.localScale = Vector3.one;

      }

      //生成fation选择图标
      for(int i=0;i< GameRoot.Instance.factionCount;i++){
         GameObject go = Instantiate(factionIcon);
         RectTransform rect = go.GetComponent<RectTransform>();
         rect.SetParent(factionPanel,false);
         rect.localScale = Vector3.one;
         SetSprite(GetImage(go.transform.Find("Mask/Icon")), "UI/UISprite/faction_" + i.ToString());


         OnClick(go,ClickFactionIcon,i);

      }
      OnClick(readyButton.gameObject, ClickReadyButton);
      OnClick(unReadyButton.gameObject, ClickUnReadyButton);
      Debug.Log("selectwindow Init");
   }




   /// <summary>
   /// 更新玩家卡片
   /// </summary>
   /// <param name="message"></param>
   public void UpdateSelectData(PbMessage message){
      int index = message.Index;
      string playerName = message.Name;
      int faction = message.SelectData.Faction;
      bool ready = message.SelectData.IsReady;
      string chatMes = message.SelectData.ChatMes;
      Debug.Log("Chat mes ;  "+ message.SelectData.ChatMes);
      var player = playerPanel.GetChild(index);

      //用户头像还不会写
      // SetSprite(GetImage(PlayerPanel.GetChild(index)),)
      

      SetText(player.Find("Left/Name"),playerName);
      Image factionIcon = GetImage( player.Find("Right/faction/IconBG/factionIcon"));
      SetSprite(factionIcon,"UI/UISprite/faction_"+faction.ToString());

      if(ready)
      {
         SetActive(player.Find("Right/Ready/ReadyText"));
         SetActive(player.Find("Right/Ready/UnReadyText"),false);
         SetMaterial(GetImage(player),"UI/UIMaterial/ReadyCard");
         
      
        
      } 
      else {
          SetActive(player.Find("Right/Ready/ReadyText"),false);
         SetActive(player.Find("Right/Ready/UnReadyText"));
         ResetMaterial(GetImage(player));
      }
         


   }
   // void SetReadyMaterial(bool ready,Transform father){
   //    Image[] images = GetComponentsInChildren<Image>(false);
      
   //    foreach(var i in images){
   //       if(TryGetComponent<Image>(out Image image)){
   //          if(ready)  SetMaterial(image,"UI/UIMaterial/ReadyCard");
   //          else ResetMaterial(image);
   //       }
   //       if(TryGetComponent<Text>(out Text text)){
   //          if(ready)  SetMaterial(text,"UI/UIMaterial/ReadyCard");
   //          else ResetMaterial(text);
   //       }
   //       // SetReadyMaterial(ready,i);
   //    }
   // }


 /// <summary>
 /// 更新当前选择的factionIcon
 /// </summary>
 /// <param name="alreadyReady"> 点击ready按钮后，所有icon失效</param>
      private void UpdateFactionIcon(bool alreadyReady = false){
         if(alreadyReady){
        
            hideIconImage.SetActive(true);
               for(int i=0;i<GameRoot.Instance.factionCount;i++){         
                  GetButton(factionPanel.GetChild(i)).interactable = false;
               
             }
         }
         else{
            hideIconImage.SetActive(false);
            for(int i=0;i<GameRoot.Instance.factionCount;i++){         
                  GetButton(factionPanel.GetChild(i)).interactable = (i != currentFactionIndex);
                  SetActive(factionPanel.GetChild(i).Find("Active"), (i == currentFactionIndex));
                 
             }
         }
      
   }
   
   public void ClickFactionIcon(PointerEventData ped ,object[] args){
      currentFactionIndex = (int)args[0];
      Debug.Log(currentFactionIndex);

      UpdateFactionIcon();
      

      //点击faction图标后，使之不可再 interactable
      for(int i=0;i<GameRoot.Instance.factionCount;i++){
         
         GetButton(factionPanel.GetChild(i)).interactable = (i != currentFactionIndex);
         SetActive(factionPanel.GetChild(i).Find("Active"), (i == currentFactionIndex));
      }
      PbMessage message = new PbMessage{
         Name = NetService.Instance.Sid.ToString(),
         Cmd =  PbMessage.Types.CMD.Room,
         CmdRoom = PbMessage.Types.CmdRoom.SelectDate,
         Index = RoomSystem.Instance.index,
         SelectData = new SelectData{
            // PlayerName = _netService.Sid.ToString(),
            IsReady = false,
            Faction = currentFactionIndex,
         }
      };
      NetService.Instance.SendMessage(message);

   }
   public void ClickReadyButton(PointerEventData ped ,object[] args){
      
         SetActive(unReadyButton);
         SetActive(readyButton,false);
         PbMessage message = new PbMessage{
         Cmd =  PbMessage.Types.CMD.Room,
         CmdRoom = PbMessage.Types.CmdRoom.SelectDate,
         Index = RoomSystem.Instance.index,
         SelectData =  new SelectData{           
            IsReady = true,    
             Faction = currentFactionIndex,    
         },
         
         
      };
      NetService.Instance.SendMessage(message);
      UpdateFactionIcon(true);


      
   }
   public void ClickUnReadyButton(PointerEventData ped ,object[] args){
         SetActive(readyButton);
         SetActive(unReadyButton, false);

         

         PbMessage message = new PbMessage{
         Cmd =  PbMessage.Types.CMD.Room,
         CmdRoom = PbMessage.Types.CmdRoom.SelectDate,

         Index = RoomSystem.Instance.index,
         SelectData =  new SelectData{
             IsReady = false,
              Faction = currentFactionIndex,
         },
         
      };
      NetService.Instance.SendMessage(message);
      UpdateFactionIcon();
   }

}
