using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class SlidePage : MonoBehaviour, IBeginDragHandler, IEndDragHandler {

    private ScrollRect scrollRect;
    private Button btnLeft;
    private Button btnRight;
    private Transform content;
    private const float check = 0.001f;

    private float smoothSpeed = 1.0f;
    private const float SMOOTH_TIME = 0.2f;
    private const float SMOOTH_SPEED = 1f;
    private float targetNor = 0.5f;
    private float velocity = 0f;

    private EDragState dragState = EDragState.Free;
    private enum EDragState{
        Free,
        Drag,
        DragFix,
    }
    private enum EDirection {
        Left,
        Right,
    }

    public int GetFocusIndex() {
        int low = 0, high = 0;
        float d = 1.0f;
        CalRange(ref low, ref high, ref d);
        return low;
    }

    public void FocusIndex(int index) {

        targetNor = CalClickNormalizeValue(index);
        dragState = EDragState.DragFix;
    }

    private void Awake() {
        scrollRect = GetComponent<ScrollRect>();
        content = transform.Find("Viewport/Content");
        btnLeft = transform.Find("ButtonL").GetComponent<Button>();
        btnRight = transform.Find("ButtonR").GetComponent<Button>();
        btnLeft.onClick.AddListener(delegate { OnBtnClick(EDirection.Left); });
        btnRight.onClick.AddListener(delegate { OnBtnClick(EDirection.Right); });
    }

    private void OnEnable() {
        UpdateBtnLRStatus();
    }


    private void Update() {
        switch (dragState) {
            case EDragState.DragFix:
                ToSpecifyPage();
                break;
            case EDragState.Drag:
            case EDragState.Free:
            default:
                break; 
        }
    }

    private void ToSpecifyPage() {
        if (Mathf.Abs(scrollRect.normalizedPosition.x - targetNor) <= check) {
            scrollRect.normalizedPosition = new Vector2(targetNor, targetNor);
            dragState = EDragState.Free;
            UpdateBtnLRStatus();
        }

        float normalizeValue = scrollRect.normalizedPosition.x;
        normalizeValue = Mathf.SmoothDamp(normalizeValue, targetNor, ref velocity, SMOOTH_TIME, SMOOTH_SPEED);
        scrollRect.normalizedPosition = new Vector2(normalizeValue, normalizeValue);
    }

    private void UpdateBtnLRStatus() {
        bool hideL = scrollRect.normalizedPosition.x < check;
        bool hideR = scrollRect.normalizedPosition.x < 1 - check;
        btnLeft.gameObject.SetActive(!hideL);
        btnRight.gameObject.SetActive(hideR);
    }

    private float CalDragNormalizeValue() {
        float normalizeValue = scrollRect.normalizedPosition.x;
        int low =0, high=0;
        float d = 1.0f;
        CalRange(ref low, ref high, ref d);
        normalizeValue = Near(normalizeValue, low * d, high * d);
        normalizeValue = Mathf.Clamp(normalizeValue, 0f, 1f);
        return normalizeValue;
    }

    private float CalClickNormalizeValue(int index) {
        float normalizeValue = 0f;
        if (content.childCount > 1) {
            normalizeValue = 1.0f * index / (content.childCount - 1);
        }
        scrollRect.normalizedPosition = new Vector2(normalizeValue, normalizeValue);
        return normalizeValue;
    }

    private void CalRange(ref int low, ref int high, ref float d) {
        if (content.childCount <= 1) {
            low = 0;
            high = 0;
            d = 1f;
            return;
        }

        float normalizeValue = scrollRect.normalizedPosition.x;
        normalizeValue = Mathf.Clamp(normalizeValue, 0f, 1f);
        d = 1.0f / (content.childCount - 1);
        low = Mathf.RoundToInt(normalizeValue / d);
        high = low + 1;
    }

    private float Near(float value, float a, float b) {
        float mid = (a+b)/2;
        return value <= mid ? a : b;
    }

    private void OnBtnClick(EDirection dir) {
        int curIndex = GetFocusIndex();
        int next = dir == EDirection.Left ? curIndex - 1 : curIndex + 1;
        next = Mathf.Clamp(next, 0, content.childCount-1);
        FocusIndex(next);
    }

    public void OnEndDrag(PointerEventData eventData) {
        targetNor = CalDragNormalizeValue();
        dragState = EDragState.DragFix;
    }

    public void OnBeginDrag(PointerEventData eventData) {
        dragState = EDragState.Drag;
    }
}
