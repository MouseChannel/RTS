using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pb;


public class LoadWindow : WindowRoot
{
    protected override void InitWindow(){
      base.InitWindow();
   }

    [SerializeField]
    private Image penrcentImage;
    [SerializeField]
    private Text penrcentText;

    public void UpdateLoadPercent(PbMessage message ){
        penrcentImage.fillAmount = 0.33f;
        penrcentText.text = "33%";
    }





}
