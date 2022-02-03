using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class WindowRoot : MonoBehaviour
{
    protected GameRoot root;
    protected NetService _netService;
    protected ResourceService _resourceService;
    protected AudioService _audioService;

    public virtual void SetWindowState(bool isActive = true){
        if(gameObject.activeSelf != isActive){
            gameObject.SetActive(isActive);
        }
        if(isActive){
            InitWindow();
        }
        else{
            UnInitWindow();
        }
    }
    // void Start(){
    //     InitWindow();
    // }

    protected virtual void InitWindow(){
        root = GameRoot.Instance;
        _audioService = AudioService.Instance;
        _netService = NetService.Instance;
        _resourceService = ResourceService.Instance;
    }
    protected void UnInitWindow(){
        root = null;
        _audioService = null;
        _netService = null;
        _resourceService = null;
        
    }

    protected void SetActive(GameObject go, bool state = true) {
        go.SetActive(state);
    }
    protected void SetActive(Transform trans, bool state = true) {
        trans.gameObject.SetActive(state);
    }
    protected void SetActive(RectTransform rectTrans, bool state = true) {
        rectTrans.gameObject.SetActive(state);
    }
    protected void SetActive(Image img, bool state = true) {
        img.gameObject.SetActive(state);
    }
    protected void SetActive(Text txt, bool state = true) {
        txt.gameObject.SetActive(state);
    }
    protected void SetActive(InputField ipt, bool state = true) {
        ipt.gameObject.SetActive(state);
    }

    protected void SetText(Transform trans, int num = 0) {
        SetText(trans.GetComponent<Text>(), num.ToString());
    }
    protected void SetText(Transform trans, string context = "") {
        SetText(trans.GetComponent<Text>(), context);
    }
    protected void SetText(Text txt, int num = 0) {
        SetText(txt, num.ToString());
    }
    protected void SetText(Text txt, string context = "") {
        txt.text = context;
    }

    protected void SetSprite(Image image, string path) {
        Sprite sp = ResourceService.Instance.LoadSprite(path, true);
        image.sprite = sp;
    }
    protected void SetMaterial(Image image, string path) {
        
        image.material = ResourceService.Instance.LoadMaterial(path, true);
       
    }
    protected void SetMaterial(Text text, string path) {
        
        text.material = ResourceService.Instance.LoadMaterial(path, true);
        
    }
        protected void ResetMaterial(Image image) {
        
        image.material = null;
       
    }
    protected void ResetMaterial(Text text) {
        
        text.material = null;
        
    }

    protected Transform GetTrans(Transform trans, string name) {
        if(trans != null) {
            return trans.Find(name);
        }
        else {
            return transform.Find(name);
        }
    }
    protected Image GetImage(Transform trans, string path) {
        if(trans != null) {
            return trans.Find(path).GetComponent<Image>();
        }
        else {
            return transform.Find(path).GetComponent<Image>();
        }
    }
    protected Image GetImage(Transform trans) {
        if(trans != null) {
            return trans.GetComponent<Image>();
        }
        else {
            return transform.GetComponent<Image>();
        }
    }
    protected Button GetButton(Transform trans) {
        if(trans != null) {
            return trans.GetComponent<Button>();
        }
        else {
            return transform.GetComponent<Button>();
        }
    }
        protected Button GetButton(Transform trans, string path) {
        if(trans != null) {
            return trans.Find(path).GetComponent<Button>();
        }
        else {
            return transform.Find(path).GetComponent<Button>();
        }
    }

    protected Text GetText(Transform trans, string path) {
        if(trans != null) {
            return trans.Find(path).GetComponent<Text>();
        }
        else {
            return transform.Find(path).GetComponent<Text>();
        }
    }

    private T GetOrAddComponent<T>(GameObject go) where T : Component {
        T t = go.GetComponent<T>();
        if(t == null) {
            t = go.AddComponent<T>();
        }
        return t;
    }
    protected void OnClick(GameObject go, Action<PointerEventData, object[]> clickCB, params object[] args) {
        UIListener listener = GetOrAddComponent<UIListener>(go);
        listener.onClick = clickCB;
        if(args != null) {
            listener.args = args;
        }
    }
    protected void OnClickDown(GameObject go, Action<PointerEventData, object[]> clickDownCB, params object[] args) {
        UIListener listener = GetOrAddComponent<UIListener>(go);
        listener.onClickDown = clickDownCB;
        if(args != null) {
            listener.args = args;
        }
    }
    protected void OnClickUp(GameObject go, Action<PointerEventData, object[]> clickUpCB, params object[] args) {
        UIListener listener = GetOrAddComponent<UIListener>(go);
        listener.onClickUp = clickUpCB;
        if(args != null) {
            listener.args = args;
        }
    }
    protected void OnDrag(GameObject go, Action<PointerEventData, object[]> dragCB, params object[] args) {
        UIListener listener = GetOrAddComponent<UIListener>(go);
        listener.onDrag = dragCB;
        if(args != null) {
            listener.args = args;
        }
    }






}
