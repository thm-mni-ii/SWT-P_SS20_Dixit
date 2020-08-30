/* created by: SWT-P_SS_20_Dixit */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Objects of this class are to store notifications, their message and their type.
/// </summary>
/// \author SWT-P_SS_20_Dixit
public class Notification
{
    /// <summary>
    /// The different types of notifications
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public enum NotificationTypes
    {
        regular,
        warning,
        good,
        bad
    }
    /// <summary>
    /// The type of this notification
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public NotificationTypes notificationType;
    /// <summary>
    /// The long, written out message of this notification
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public string notificationLong;
    /// <summary>
    /// A short summary of the message of this notification
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public string notificationShort;

    /// <summary>
    /// The constructor
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public Notification(NotificationTypes notificationType, string notificationLong, string notificationShort)
    {
        this.notificationType = notificationType;
        this.notificationLong = notificationLong;
        this.notificationShort = notificationShort;
    }

    /// <summary>
    /// The default constructor
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public Notification()
    {
        this.notificationType = NotificationTypes.regular;
        this.notificationLong = "DefaultLongNotification";
        this.notificationShort = "Default";
    }
}
