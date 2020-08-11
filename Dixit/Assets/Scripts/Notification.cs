using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Notification
{
    public enum NotificationTypes
    {
        regular,
        warning,
        good,
        bad
    }
    
    public NotificationTypes notificationType;
    public string notificationLong;
    public string notificationShort;

    public Notification(NotificationTypes notificationType, string notificationLong, string notificationShort)
    {
        this.notificationType = notificationType;
        this.notificationLong = notificationLong;
        this.notificationShort = notificationShort;
    }

    public Notification()
    {
        this.notificationType = NotificationTypes.regular;
        this.notificationLong = "DefaultLongNotification";
        this.notificationShort = "Default";
    }
}
