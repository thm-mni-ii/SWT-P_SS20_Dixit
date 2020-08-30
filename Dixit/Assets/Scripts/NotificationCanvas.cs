﻿using System;
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
    private Vector3 _supposedPosition;

    private Vector2 initialSize;
   
    [SerializeField]
    private Animator animator;

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
        _tmpug.text = notification.notificationShort;
    }
    /// <summary>
    /// Causes the notification canvas to display the short notification
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public void showShort()
    {
        animator.Play("Shrink");
        _tmpug.text = notification.notificationShort;
    }
    /// <summary>
    /// Causes the notification canvas to display the long notification
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public void showLong()
    {
        animator.Play("Grow");
        _tmpug.text = notification.notificationLong;
    }

    private void OnMouseEnter()
    {
        showLong();
    }

    private void OnMouseExit()
    {
        showShort();
    }

    public void FadeOut()
    {
        animator.Play("FadeOut");
    }

    private void OnMouseDown()
    {
        notificationSystem.RemoveNotification();
    }
    /// <summary>
    /// Slides the notification to a given location.
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public IEnumerator slideToSupposedPosition(Vector3 supposedPosition)
    {
        _supposedPosition = supposedPosition;
        while (transform.localPosition != _supposedPosition)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, _supposedPosition, Time.deltaTime * 20);
            yield return new WaitForSeconds(Time.deltaTime);
        }
    }
}
