using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// A system to manage and show notifications.
/// </summary>
/// \author SWT-P_SS_20_Dixit
public class NotificationSystem : MonoBehaviour
{
    /// <summary>
    /// The Notification GameObject to be spawned.
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public GameObject NotificationPrefab;
    /// <summary>
    /// A canvas/rect on which notifications are to be spawned
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public RectTransform notificationSpace;
    /// <summary>
    /// The max number of notifications to be displayed at once
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    const int maxNotifications = 5;

    private float notificationHeight;
    private float notificationWidth;

    /// <summary>
    /// The colors the different notification zypes are to be displayed in
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public Color regularColor = Color.black;
    public Color warningColor = Color.yellow;
    public Color goodColor = Color.green;
    public Color badColor = Color.red;

    private Queue<GameObject> notifications = new Queue<GameObject>();

    public AudioSource goodsound;
    public AudioSource badsound;

    /// <summary>
    /// Adds a notification to the queue and displays it
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public void addNotification(Notification notification)
    {
        if (notifications.Count == maxNotifications)
        {
            Destroy(notifications.Dequeue());

        }
        GameObject notif = Instantiate(NotificationPrefab,notificationSpace);
        notif.GetComponentInChildren<NotificationCanvas>().notification=notification;
        notif.GetComponent<RectTransform>().sizeDelta = new Vector2(notificationWidth,notificationHeight);
        notif.GetComponent<NotificationCanvas>().init();
        notifications.Enqueue(notif);
        if (notification.notificationType == Notification.NotificationTypes.good)
        {
            goodsound.Play();
        }
        else
        {
            badsound.Play();
        }

        updateNotifications();
    }

    /// <summary>
    /// Moves all notifications to their respective positions
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public void updateNotifications()
    {
        int i = 0;
        foreach (GameObject notif in notifications)
        {
            Debug.Log("penis");
            StartCoroutine(notif.GetComponent<NotificationCanvas>().slideToSupposedPosition(new Vector3(0,-(i * (notificationSpace.rect.height / maxNotifications)))));
            i++;
        }
    }

    /// <summary>
    /// Dequeues and destroys the oldest notification
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public void RemoveNotification()
    { 
        notifications.Peek().GetComponent<NotificationCanvas>().FadeOut();
        Invoke(nameof(DestroyNotificationAndUpdate),0.25f);
    }

    private void DestroyNotificationAndUpdate()
    {
        Destroy(notifications.Dequeue());
        updateNotifications();
    }

    // Start is called before the first frame update
    void Start()
    {
        notificationWidth = notificationSpace.rect.width;
        notificationHeight = (notificationSpace.rect.height / maxNotifications)-2;
    }

}
