using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NotificationSystem : MonoBehaviour
{
    public GameObject NotificationPrefab;
    public RectTransform notificationSpace;
    const int maxNotifications = 5;

    private float notificationHeight;
    private float notificationWidth;
    
    public Color regularColor = Color.black;
    public Color warningColor = Color.yellow;
    public Color goodColor = Color.green;
    public Color badColor = Color.red;

    private Queue<GameObject> notifications = new Queue<GameObject>();

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
        
        updateNotifications();
    }

    public void updateNotifications()
    {
        int i = 0;
        foreach (GameObject notif in notifications)
        {
            notif.GetComponent<RectTransform>().localPosition = new Vector3(0,-(i * (notificationSpace.rect.height / maxNotifications)));
            i++;
        }
    }

    public void RemoveNotification()
    {
        Debug.Log("Count:"+notifications.Count);
        GameObject temp = notifications.Dequeue();
        Destroy(temp);
        updateNotifications();
    }

    /*public void compressNotifications()
    {
        List<GameObject> temp = new List<GameObject>(maxNotifications);
        foreach (GameObject notif in notifications)
        {
            if(notif != null)
            temp.Add(notif);
        }

        notifications = temp;
    }*/

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
