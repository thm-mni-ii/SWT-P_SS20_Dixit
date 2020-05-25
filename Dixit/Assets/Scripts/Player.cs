using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class Player : NetworkBehaviour
{
    private int points {get; set;}
    private string playerName {get; set;}

    private GameManager gameManager;

    public void Start()
    {
        var nm = NetowrkManager.singleton.gameObject;

        gameManager = nm.GetComponent<NetowrkManager>().gameManager.GetComponent<GameManager>();
    }

    [Command]
    public void CmdGiveAnswer(string answer)
    {
        gameManager.LogAnswer(this.netIdentity, answer);
    }

    [Command]
    public void CmdChooseAnswer(int answer)
    {
        gameManager.LogAnswer(this.netIdentity, answer);
    }

}
