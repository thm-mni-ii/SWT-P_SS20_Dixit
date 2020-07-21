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
public class Player : NetworkBehaviour
{
    private static readonly Lazy<Player> _localPlayer = new Lazy<Player>(() => ClientScene.localPlayer.gameObject.GetComponent<Player>());
    public static Player LocalPlayer => _localPlayer.Value;

    public string PlayerName { get; set; }

    public GameManager gameManager;
    private GameObject notifictionCanvas;

    private Card _selectedCard = null;
    public Card SelectedCard
    {
        set {_selectedCard = value;}
        private get {return _selectedCard;}
    }
    private bool messageActive = false;

    /// <summary>
    /// Called when the local Player Object has been set up
    /// </summary>
    public override void OnStartServer()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    [Client]
    public override void OnStartLocalPlayer()
    {
        //Initialzie notification system
        notifictionCanvas = GameObject.Find("NotificationCanvas");
        notifictionCanvas.SetActive(false);
    }

    /// <summary>
    /// Sends the given Answer from a Player to the GameManager.
    /// <param name="answer">The Answer.</param>
    /// </summary>
    [Command]
    public void CmdGiveAnswer(string answer)
    {
        gameManager.LogAnswer(this.netIdentity.netId, answer);
    }

    /// <summary>
    /// Sends the choosen Answer from a Player to the GameManager
    /// <param name="answer">The Answer</param>
    /// </summary>
    [Command]
    private void CmdChooseAnswer(UInt32 answer)
    {
        gameManager.LogAnswer(this.netIdentity.netId, answer);
    }

    [Client]
    public void ChooseAnswer(Card card)
    {
        SelectedCard?.HighlightReset();
        SelectedCard = card;
        CmdChooseAnswer(card.id);
    }

    [Command]
    public void CmdPlayerIsReady()
    {
        gameManager.LogPlayerIsReady();
    }


    [TargetRpc]
    public void TargetSendNotification(string massage)
    {
        var notifiction = notifictionCanvas.GetComponentsInChildren<TextMeshProUGUI>()[0];
        notifiction.text = messageActive?  notifiction.text + "\n---\n" + massage  : massage;
        StartCoroutine(showNotificationAndWait(5));
    }

    [Client]
    private IEnumerator showNotificationAndWait(int time)
    {
        notifictionCanvas.SetActive(true);

        if(messageActive)
        {
            var wait = System.Diagnostics.Stopwatch.StartNew();
            while (messageActive)
            {
                yield return null;
            }
            notifictionCanvas.SetActive(true);
            wait.Stop();

            time -= (int) (wait.ElapsedMilliseconds / 1000);
        }

        messageActive = true;
        yield return new WaitForSeconds(time);
        notifictionCanvas.SetActive(false);
        messageActive = false;
    }

    public void KillGame()
    {
        Application.Quit();
    }   

    [Command]
    public void CmdRestart()
    {
        gameManager.Restart();
    }

}
