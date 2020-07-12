﻿/* created by: SWT-P_SS_20_Dixit */
using System;
using System.Collections;
using System.Linq;
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
    private static readonly Lazy<Player> _localPlayer = new Lazy<Player>(() => ClientScene.localPlayer.gameObject.GetComponent<Player>());
    public static Player LocalPlayer => _localPlayer.Value;

    public string PlayerName { get; set; }

    public GameManager gameManager;
    public GameObject[] PlayerCanvasEntry = new GameObject[5];
    private GameObject resultOverlayCanvas;
    private GameObject notifictionCanvas;
    public GameObject[] TextPanelEntry = new GameObject[5];
    private TextMeshProUGUI ScoreHeader;

    private Card selectedCard = null;
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
        //Initialize all variables necessary for PlayerCanvas control
        for (int i = 0; i < PlayerCanvasEntry.Length; i++) {
            PlayerCanvasEntry[i] = GameObject.Find($"PlayerCanvasPlayer{i+1}");
        }

        //Initialize all variables necessary for ScoreCanvas control
        resultOverlayCanvas = GameObject.FindGameObjectWithTag("ScoreResultOverlay");
        GameObject BGPanel = GameObject.Find("BGPanel");
        ScoreHeader = BGPanel.GetComponentInChildren<TextMeshProUGUI>();

        for (int i = 0; i < TextPanelEntry.Length; i++) {
            TextPanelEntry[i] = GameObject.Find($"ScoreResultOverlayPlayer{i+1}");
        }

        resultOverlayCanvas.SetActive(false);

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
        selectedCard?.HighlightReset();
        selectedCard = card;
        CmdChooseAnswer(card.id);
    }

    [Command]
    public void CmdPlayerIsReady()
    {
        gameManager.LogPlayerIsReady();
    }

    [TargetRpc]
    public void TargetResultOverlaySetActive(bool isActive)
    {
        resultOverlayCanvas.GetComponentInChildren<Button>().interactable = true;
        resultOverlayCanvas.SetActive(isActive);
        selectedCard = null;
    }

    [ClientRpc]
    public void RpcDeleteInputCard()
    {
        Destroy(GameObject.FindGameObjectsWithTag("InputCard")[0]);
    }

    [ClientRpc]
    public void RpcDeleteQuestionCard()
    {
        Destroy(GameObject.FindGameObjectsWithTag("QuestionCard")[0]);
    }

    [ClientRpc]
    public void RpcDeleteAllAnswerCards()
    {
        var answerCards = GameObject.FindGameObjectsWithTag("AnswerCard");

        for (int i = 0; i < answerCards.Length; i++)
        {
            Destroy(answerCards[i]);
        }
    }

    [ClientRpc]
    public void RpcHighlightCard(UInt32 correctCard)
    {
        var cards = GameObject.FindGameObjectsWithTag("AnswerCard").Select(go => go.GetComponent<Card>());
        foreach (var card in cards)
        {
            card.DisableSelectInput();

            if (card.id == correctCard)
            {
                card.HighlightCorrect();
            }
        }
    }

    /// <summary>
    /// Updates a PlayerCanvasEntry with given index, playername and score
    /// </summary>
    [TargetRpc]
    public void TargetUpdatePlayerCanvasEntry(int idx, string player, string points)
    {
        TextMeshProUGUI[] entry = PlayerCanvasEntry[idx].GetComponentsInChildren<TextMeshProUGUI>();
        entry[0].text = player;
        entry[1].text = points;
    }

    /// <summary>
    /// Updates a TextPanelEntry (in ScoreResultOverlay) with given index, playername and score
    /// Shows a + befor positive values.
    /// For round point view.
    /// </summary>
    [Server]
    public void UpdateTextPanelEntry(int idx, string player, int points) =>
        TargetUpdateTextPanelEntry(idx, player, points, false);

    /// <summary>
    /// Updates a TextPanelEntry (in ScoreResultOverlay) with given index, playername and score
    /// Shows no + before positive values.
    /// For final point view.
    /// </summary>
    [Server]
    public void UpdateTextPanelEntryGameEnd(int idx, string player, int points) =>
        TargetUpdateTextPanelEntry(idx, player, points, true);

    /// <summary>
    /// Updates a TextPanelEntry (in ScoreResultOverlay) with given index, playername and score.
    /// Shows a + befor positive values if gameEnd is false.
    /// </summary>
    [TargetRpc]
    private void TargetUpdateTextPanelEntry(int idx, string player, int points, bool gameEnd)
    {
        TextMeshProUGUI[] entry = TextPanelEntry[idx].GetComponentsInChildren<TextMeshProUGUI>(true);
        entry[0].enabled = true;
        entry[1].text = player;
        entry[2].text = ((!gameEnd && points > 0) ? "+" : "") + points;
    }

    /// <summary>
    /// Updates a ScoreHeader (in ScoreResultOverlay) with given roundNumber
    /// </summary>
    [TargetRpc]
    public void TargetUpdateScoreHeader(int roundNumber)
    {
        ScoreHeader.text = $"~ Punkte in Runde {roundNumber} ~";
    }

    /// <summary>
    /// Updates a ScoreHeader (in ScoreResultOverlay) for the end of the game
    /// </summary>
    [TargetRpc]
    public void TargetUpdateScoreHeaderGameEnd()
    {
        ScoreHeader.text = "~ Gesamtpunkte ~";
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

}
