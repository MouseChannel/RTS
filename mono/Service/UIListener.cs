

using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIListener :
    MonoBehaviour,
    IPointerClickHandler,
    IPointerDownHandler,
    IPointerUpHandler,
    IDragHandler {
    public Action<PointerEventData, object[]> onClick;
    public Action<PointerEventData, object[]> onClickDown;
    public Action<PointerEventData, object[]> onClickUp;
    public Action<PointerEventData, object[]> onDrag;

    public object[] args = null;

    public void OnPointerClick(PointerEventData eventData) {
        onClick?.Invoke(eventData, args);
    }
    public void OnPointerDown(PointerEventData eventData) {
        onClickDown?.Invoke(eventData, args);
    }
    public void OnPointerUp(PointerEventData eventData) {
        onClickUp?.Invoke(eventData, args);
    }
    public void OnDrag(PointerEventData eventData) {
        onDrag?.Invoke(eventData, args);
    }
}
