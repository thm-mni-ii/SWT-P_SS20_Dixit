using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// Simple class for storing player data.
/// Similar to UserInfo may replace
/// </summary>
public class PlayerInfo
{
    public PlayerInfo(string name, bool isHost)
    {
        this.name = name;
        this.isHost = isHost;
    }
    public string name;
    public bool isHost;
}