using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

public class DisplayManager : NetworkBehaviour
{
    public TextMeshProUGUI ScoreHeader;
    public GameObject[] PlayerCanvasEntry = new GameObject[5];
    public GameObject[] TextPanelEntry = new GameObject[5];

    public GameObject resultOverlayCanvas;

    public GameObject exitButton;
    public GameObject restartButton;
    public GameObject continueButton;

    [Client]
    public override void OnStartLocalPlayer()
    {
        exitButton.SetActive(false);
        restartButton.SetActive(false);
    }

    /// <summary>
    /// Updates the ScoreHeader (in ScoreResultOverlay) with given roundNumber
    /// </summary>
    [Server]
    public void UpdateScoreHeader(int roundNumber) =>
        RpcUpdateScoreHeaderText($"~ Punkte in Runde {roundNumber} ~");

    /// <summary>
    /// Updates the ScoreHeader (in ScoreResultOverlay) with given text
    /// </summary>
    [ClientRpc]
    public void RpcUpdateScoreHeaderText(string text) =>
        ScoreHeader.text = text;

    /// <summary>
    /// Updates a PlayerCanvasEntry with given index, playername and score
    /// </summary>
    [ClientRpc]
    public void RpcUpdatePlayerCanvasEntry(int idx, string player, string points)
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
        RpcUpdateTextPanelEntry(idx, player, points, false);

    /// <summary>
    /// Updates a TextPanelEntry (in ScoreResultOverlay) with given index, playername and score
    /// Shows no + before positive values.
    /// For final point view.
    /// </summary>
    [Server]
    public void UpdateTextPanelEntryGameEnd(int idx, string player, int points) =>
        RpcUpdateTextPanelEntry(idx, player, points, true);

    /// <summary>
    /// Updates a TextPanelEntry (in ScoreResultOverlay) with given index, playername and score.
    /// Shows a + befor positive values if gameEnd is false.
    /// </summary>
    [ClientRpc]
    private void RpcUpdateTextPanelEntry(int idx, string player, int points, bool gameEnd)
    {
        TextMeshProUGUI[] entry = TextPanelEntry[idx].GetComponentsInChildren<TextMeshProUGUI>(true);
        entry[0].enabled = true;
        entry[1].text = player;
        entry[2].text = ((!gameEnd && points > 0) ? "+" : "") + points;
    }

    /// <summary>
    /// Switches between continue button (isActive = false) and exit + restart buttons (isActive = false).
    /// </summary>
    [ClientRpc]
    public void RpcToggleRestartExit(bool isActive)
    {
        continueButton.SetActive(!isActive);
        exitButton.SetActive(isActive);
        restartButton.SetActive(isActive);
    }

    /// <summary>
    /// Opens the result overlay for all players.
    /// </summary>
    [ClientRpc]
    public void RpcResultOverlaySetActive(bool isActive)
    {
        continueButton.GetComponent<Button>().interactable = true;
        resultOverlayCanvas.SetActive(isActive);
        ClientScene.localPlayer.GetComponent<Player>().SelectedCard = null;
    }

    /// <summary>
    /// Removes the input card for all players.
    /// </summary>
    [ClientRpc]
    public void RpcDeleteInputCard()
    {
        Destroy(GameObject.FindGameObjectsWithTag("InputCard")[0]);
    }

    /// <summary>
    /// Removes the question card for all players.
    /// </summary>
    [ClientRpc]
    public void RpcDeleteQuestionCard()
    {
        Destroy(GameObject.FindGameObjectsWithTag("QuestionCard")[0]);
    }

    /// <summary>
    /// Removes all answer cards for all players.
    /// </summary>
    [ClientRpc]
    public void RpcDeleteAllAnswerCards()
    {
        var answerCards = GameObject.FindGameObjectsWithTag("AnswerCard");

        for (int i = 0; i < answerCards.Length; i++)
        {
            Destroy(answerCards[i]);
        }
    }

    /// <summary>
    /// Highlights the correct answer cards for all players.
    /// </summary>
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
}
