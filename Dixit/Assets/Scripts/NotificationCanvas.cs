using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NotificationCanvas : MonoBehaviour
{
    [NonSerialized]public Notification notification;

    private Vector2 initialSize;

    private RectTransform _rectTransform;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void init()
    {
        _rectTransform = GetComponent<RectTransform>();
        initialSize = _rectTransform.sizeDelta;
        showShort();
    }

    public void showShort()
    {
        GetComponentInChildren<TextMeshProUGUI>().text = notification.notificationShort;
        _rectTransform.sizeDelta = initialSize;
    }
    public void showLong()
    {
        GetComponentInChildren<TextMeshProUGUI>().text = notification.notificationLong;
        _rectTransform.sizeDelta = initialSize*new Vector2(3,1);
    }

    private void OnMouseEnter()
    {
        Debug.Log("ONMOUSEENTER");
        showLong();
    }

    private void OnMouseExit()
    {
        showShort();
    }
}
