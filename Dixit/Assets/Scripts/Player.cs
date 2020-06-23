/* created by: SWT-P_SS_20_Dixit */
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

/// <summary>
/// Represents a Player in the Game.
/// Communicates with the GameManager to log Answers and change the current Game Phase.
/// </summary>
public class Player : NetworkBehaviour
{
    private static Lazy<Player> _localPlayer = new Lazy<Player>(() => ClientScene.localPlayer.gameObject.GetComponent<Player>());
    public static Player LocalPlayer => _localPlayer.Value;

    private int points { get; set; }
    public string playerName { get; set; }

    public GameManager gameManager;
    public GameObject[] PlayerCanvasEntry = new GameObject[5];
    public Canvas PlayerCanvas;

    /// <summary>
    /// Called when the local Player Object has been set up
    /// </summary>
    public override void OnStartServer()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    public override void OnStartClient(){
        PlayerCanvas = GameObject.Find("UICanvas").GetComponent<Canvas>().GetComponentsInChildren<Canvas>()[0];
        PlayerCanvasEntry[0]=GameObject.Find("Player1st");
        PlayerCanvasEntry[1]=GameObject.Find("Player2nd");
        PlayerCanvasEntry[2]=GameObject.Find("Player3rd");
        PlayerCanvasEntry[3]=GameObject.Find("Player4th");
        PlayerCanvasEntry[4]=GameObject.Find("Player5th");
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
    public void CmdChooseAnswer(NetworkIdentity answer)
    {
        gameManager.LogAnswer(this.netIdentity, answer);
    }

    [ClientRpc]
    public void RpcDeleteInputCard()
    {
        Destroy(GameObject.FindGameObjectsWithTag("InputCard")[0]);
    }

    [ClientRpc]
    public void RpcHighlightCard(NetworkIdentity correctCard)
    {
        var cards = GameObject.FindGameObjectsWithTag("AnswerCard").Select(go => go.GetComponent<Card>());
        foreach(var card in cards)
        {
            if(card.choosen == correctCard)
            {
                card.HighlightCorrect();
            }
        }
    }

    /// <summary>
    /// Updates a PlayerCanvasEntry with given index, playername and score
    /// </summary>
    [TargetRpc]
    public void TargetUpdatePlayerCanvasEntry(int idx, String player, String points){
        TextMeshProUGUI[] entry = PlayerCanvasEntry[idx].GetComponentsInChildren<TextMeshProUGUI>();
        entry[0].text = player;
        entry[1].text = points;
    }

}
