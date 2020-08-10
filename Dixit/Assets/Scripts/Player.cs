﻿/* created by: SWT-P_SS_20_Dixit */
using System;
using System.Collections;
using UnityEngine;
using Mirror;
using TMPro;

/// <summary>
/// Represents a Player in the Game.
/// Communicates with the GameManager to log Answers and change the current Game Phase.
/// </summary>
/// \author SWT-P_SS_20_Dixit
public class Player : NetworkBehaviour
{
    private static readonly Lazy<Player> _localPlayer = new Lazy<Player>(() => ClientScene.localPlayer.gameObject.GetComponent<Player>());
    /// <summary>
    /// The local player in each client
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public static Player LocalPlayer => _localPlayer.Value;

    /// <summary>
    /// The name of the player
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public string PlayerName { get; set; }

    /// <summary>
    /// The <c>GameManager</c>
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public GameManager gameManager;
    private GameObject notifictionCanvas;

    /// <summary>
    /// The currently selected card. Used in the "ChooseAnswer" phase
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public Card SelectedCard { set; private get; }
    private bool messageActive = false;

    /// <summary>
    /// Called when the local Player Object has been set up
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public override void OnStartServer()
    {
        gameManager = GameManager.Instance;
    }

    [Client]
    public override void OnStartLocalPlayer()
    {
        //Initialzie notification system
        notifictionCanvas = GameObject.FindGameObjectWithTag("NotificationCanvas");
        notifictionCanvas.SetActive(false);
    }

    /// <summary>
    /// Sends the given Answer from a Player to the GameManager.
    /// </summary>
    /// <param name="answer">The Answer.</param>
    /// \author SWT-P_SS_20_Dixit
    [Command]
    public void CmdGiveAnswer(string answer)
    {
        gameManager.LogAnswer(this.netIdentity.netId, answer);
    }

    /// <summary>
    /// Sends the chosen Answer from a Player to the GameManager
    /// </summary>
    /// <param name="answer">The Answer</param>
    /// \author SWT-P_SS_20_Dixit
    [Command]
    private void CmdChooseAnswer(UInt32 answer)
    {
        gameManager.LogAnswer(this.netIdentity.netId, answer);
    }

    /// <summary>
    /// Sends the answer selected during the "ChoseAnswer" phase to the <c>GameManager</c>
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    [Client]
    public void ChooseAnswer(Card card)
    {
        SelectedCard?.HighlightReset();
        SelectedCard = card;
        CmdChooseAnswer(card.id);
    }

    /// <summary>
    /// Tells the GameManger that the player clicked on the "Weiter" button
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    [Command]
    public void CmdPlayerIsReady()
    {
        gameManager.LogPlayerIsReady();
    }


    /// <summary>
    /// Displays a Notification to the client
    /// </summary>
    /// <param name="message">The content of the notification</param>
    /// \author SWT-P_SS_20_Dixit
    [TargetRpc]
    public void TargetSendNotification(string message)
    {
        var notifiction = notifictionCanvas.GetComponentsInChildren<TextMeshProUGUI>()[0];
        notifiction.text = messageActive ? notifiction.text + "\n---\n" + message : message;
        StartCoroutine(ShowNotificationAndWait(5));
    }

    /// <summary>
    /// Displays a notification for the given <c>time</c>
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    [Client]
    private IEnumerator ShowNotificationAndWait(int time)
    {
        notifictionCanvas.SetActive(true);

        if (messageActive)
        {
            var wait = System.Diagnostics.Stopwatch.StartNew();
            while (messageActive)
            {
                yield return null;
            }
            notifictionCanvas.SetActive(true);
            wait.Stop();

            time -= (int)(wait.ElapsedMilliseconds / 1000);
        }

        messageActive = true;
        yield return new WaitForSeconds(time);
        notifictionCanvas.SetActive(false);
        messageActive = false;
    }

    /// <summary>
    /// Quits the game
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public void KillGame()
    {
        Application.Quit();
    }

    /// <summary>
    /// Tells the server that this clients wants to restart the game
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    [Command]
    public void CmdRestart()
    {
        gameManager.Restart();
    }

}
