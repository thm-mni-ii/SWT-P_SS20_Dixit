/* created by: SWT-P_SS_20_Dixit */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

/// <summary>
/// Represents a Player in the Game.
/// Communicates with the GameManager to log Answers and change the current Game Phase.
/// </summary>
public class Player : NetworkBehaviour
{
    private int points {get; set;}
    private string playerName {get; set;}

    private GameManager gameManager;

    /// <summary>
    /// Called when the local Player Object has been set up
    /// </summary>
    public override void OnStartLocalPlayer()
    {
        var nm = GameServer.singleton.gameObject;

        gameManager = nm.GetComponent<GameServer>().gameManager.GetComponent<GameManager>();
    }

    /// <summary>
    /// Sends the given Answer from a Player to the GameManager.
    /// <param name="answer">The Answer.</param>
    /// </summary>
    [Command]
    public void CmdGiveAnswer(string answer)
    {
        gameManager.LogAnswer(this.netIdentity, answer);
    }

    /// <summary>
    /// Sends the choosen Answer from a Player to the GameManager
    /// <param name="answer">The Answer</param>
    /// </summary>
    [Command]
    public void CmdChooseAnswer(int answer)
    {
        gameManager.LogAnswer(this.netIdentity, answer);
    }

}
