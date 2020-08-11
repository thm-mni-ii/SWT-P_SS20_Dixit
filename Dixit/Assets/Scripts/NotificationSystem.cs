using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NotificationSystem : MonoBehaviour
{
    public GameObject NotificationPrefab;
    public RectTransform notificationSpace;
    const int maxNotifications = 4;

    private float notificationHeight;
    private float notificationWidth;
    
    public Color regularColor;
    public Color warningColor;
    public Color goodColor;
    public Color badColor;
    
    private List<GameObject> notifications = new List<GameObject>();

    public void addNotification(Notification notification)
    {
        if (notifications.Count == maxNotifications)
        {
            notifications.RemoveAt(0);
        }
        GameObject notif = Instantiate(NotificationPrefab,notificationSpace);
        notif.GetComponentInChildren<NotificationCanvas>().notification=notification;
        notif.GetComponent<RectTransform>().sizeDelta = new Vector2(notificationWidth,notificationHeight);
        notifications.Add(notif);
        notif.GetComponent<NotificationCanvas>().init();
        updateNotifications();
    }

    public void updateNotifications()
    {
        int i = 0;
        foreach (GameObject notif in notifications)
        {
            //Setzt die Position relativ zum Anker abhängig von den bestehenden notifications
            Debug.Log("Position at:"+-(notifications.Count * (notificationSpace.rect.height / maxNotifications)));
            notif.GetComponent<RectTransform>().localPosition = new Vector3(0,-(i * (notificationSpace.rect.height / maxNotifications)));
            //notif.GetComponent<RectTransform>().anchoredPosition.Set(0, -(notifications.Count * (notificationSpace.rect.height / maxNotifications)));
            i++;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        notificationWidth = notificationSpace.rect.width;
        notificationHeight = (notificationSpace.rect.height / maxNotifications)-2;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
