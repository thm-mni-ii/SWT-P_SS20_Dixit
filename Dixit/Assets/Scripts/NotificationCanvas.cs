using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class NotificationCanvas : MonoBehaviour
{
    /// <summary>
    /// The notification to be displayed on this canvas
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public Notification notification;
    /// <summary>
    /// The collider of the notification, used for mouse events.
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public BoxCollider2D collider;
    /// <summary>
    /// The notification system that manages this notification
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public NotificationSystem notificationSystem;

    private Vector2 initialSize;

    private RectTransform _rectTransform;
    private TextMeshProUGUI _tmpug;
    // Start is called before the first frame update
    void Start()
    {
        notificationSystem = GameObject.FindGameObjectWithTag("NotificationSystem").GetComponent<NotificationSystem>();
    }

    /// <summary>
    /// Initializes this object and its properties
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
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
    /// <summary>
    /// Causes the notification canvas to display the short notification
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public void showShort()
    {
        _tmpug.text = notification.notificationShort;
        _rectTransform.sizeDelta = initialSize;
    }
    /// <summary>
    /// Causes the notification canvas to display the long notification
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
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
