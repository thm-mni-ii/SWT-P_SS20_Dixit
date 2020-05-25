using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class PlayerInput : MonoBehaviour
{
    private Lazy<Player> _localPlayer = new Lazy<Player>(() => ClientScene.localPlayer.gameObject.GetComponent<Player>());

    public Player LocalPlayer => _localPlayer.Value;

    public void GiveAnswer(string answer)
    {
        LocalPlayer.CmdGiveAnswer(answer);
    }
}
