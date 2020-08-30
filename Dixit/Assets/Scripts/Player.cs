/* created by: SWT-P_SS_20_Dixit */

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
    private GameObject notificationSystem;
    
    private bool enableTutorial = true;

    private static readonly Lazy<Player> _localPlayer =
        new Lazy<Player>(() => ClientScene.localPlayer.gameObject.GetComponent<Player>());

    /// <summary>
    /// The local player in each client
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public static Player LocalPlayer => _localPlayer.Value;

    /// <summary>
    /// The name of the player
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public string PlayerName = null;

    /// <summary>
    /// The placement of the player
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public int Placement { get; set; }

    /// <summary>
    /// The <c>GameManager</c>
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public GameManager gameManager;
    private GameObject notificationCanvas;

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
        notificationSystem = GameObject.FindGameObjectWithTag("NotificationSystem");

        PlayerName = ((GameServer) NetworkManager.singleton).PlayerInfos.name;
        CmdSendName(PlayerName);
    }

    [Command]
    public void CmdSendName(string name)
    { 
        PlayerName = name;
    }

    public void GiveAnswer(string answer)
    {
        PlayerInput.singleton.canSubmit = false;
        CmdGiveAnswer(answer);
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
    public void TargetSendNotification(Notification notification)
    {
        notificationSystem.GetComponent<NotificationSystem>().addNotification(notification);
    }
    [TargetRpc]
    public void TargetSendTutorialNotification(Notification notification)
    {
        if (enableTutorial)
        {
            notificationSystem.GetComponent<NotificationSystem>().addNotification(notification);
        }
    }

    [ClientRpc]
    public void RpcSetNameOfNewPlayer(string playerName)
    {
        PlayerName = playerName;
    }

    /// <summary>
    /// Finish the game.
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    [Command]
    public void CmdFinishGame()
    {
        TargetSendResults(gameManager.GetNameOfWinner());
    }


    /// <summary>
    /// Send results to the framework
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    [TargetRpc]
    public void TargetSendResults(string winner)
    {
        GameServer.Instance.HandleGameResults(Placement,winner);
    }

    /// <summary>
    /// Sets the <c>canSubmit</c> variable in the <c>PlayerInput<c/> sigleton
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    [TargetRpc]
    public void TargetCanSubmit(bool canSubmit)
    {
        PlayerInput.singleton.canSubmit = canSubmit;
    }
}
