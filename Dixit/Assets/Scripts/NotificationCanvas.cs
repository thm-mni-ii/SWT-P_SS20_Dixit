using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class NotificationCanvas : MonoBehaviour
{
    public Notification notification;
    public BoxCollider2D collider;
    public NotificationSystem notificationSystem;

    private Vector2 initialSize;

    private RectTransform _rectTransform;
    private TextMeshProUGUI _tmpug;
    // Start is called before the first frame update
    void Start()
    {
        notificationSystem = GameObject.FindGameObjectWithTag("NotificationSystem").GetComponent<NotificationSystem>();
    }


    public void init()
    {
        _rectTransform = GetComponent<RectTransform>();
        _tmpug = GetComponentInChildren<TextMeshProUGUI>();
        initialSize = _rectTransform.sizeDelta;
        collider.offset = initialSize / -2;
        collider.size = initialSize;

        switch (notification.notificationType)
        {
            case Notification.NotificationTypes.bad:
                _tmpug.color = notificationSystem.badColor;
                break;
            case Notification.NotificationTypes.good:
                _tmpug.color = notificationSystem.goodColor;
                break;
            case Notification.NotificationTypes.regular:
                _tmpug.color = notificationSystem.regularColor;
                break;
            case Notification.NotificationTypes.warning:
                _tmpug.color = notificationSystem.warningColor;
                break;
        }
        
        showShort();
    }

    public void showShort()
    {
        _tmpug.text = notification.notificationShort;
        _rectTransform.sizeDelta = initialSize;
    }
    public void showLong()
    {
        _tmpug.text = notification.notificationLong;
        _rectTransform.sizeDelta = initialSize*new Vector2(2.7f,1);
    }

    private void OnMouseEnter()
    {
        showLong();
    }

    private void OnMouseExit()
    {
        showShort();
    }

    private void OnMouseDown()
    {
        notificationSystem.RemoveNotification();
    }
}
