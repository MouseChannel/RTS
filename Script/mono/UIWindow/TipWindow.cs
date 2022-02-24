using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TipWindow : WindowRoot{
    public Text txtTips;
    // [SerializeField]
    // private Image successfulIcon, failedIcon, messageIcon;
    private bool isTipsShow = false;
    private Queue<string> tipsQue = new Queue<string>();
    public override void InitWindow() { 
        base.InitWindow();
        tipsQue.Clear();
    }

    private void Update() {
        if(tipsQue.Count > 0 && isTipsShow == false) {
            string tips = tipsQue.Dequeue();
            isTipsShow = true;
            SetTips(tips);
        }
    }

    private void SetTips(string tips) {
        int len = tips.Length;
        txtTips.text = tips;
        // bgTips.GetComponent<RectTransform>().sizeDelta = new Vector2(35 * len + 100, 80);

       
    }

    public void AddTips(string tips) {
        tipsQue.Enqueue(tips);
    }

    public void AniPlayDone() {
        // SetActive(bgTips, false);
        // isTipsShow = false;
    }
}
