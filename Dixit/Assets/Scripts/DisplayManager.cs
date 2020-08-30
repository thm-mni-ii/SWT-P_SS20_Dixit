/* created by: SWT-P_SS_20_Dixit */

using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

/// <summary>
/// Shows, hides and updates UI elements on screen.
/// Also deletes and updates cards after they have been spawen by the <c>GameManager</c>.
/// \author SWT-P_SS_20_Dixit
/// </summary>
public class DisplayManager : NetworkBehaviour
{
    /// <summary>
    /// Headline of the score Overlay UI element
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public TextMeshProUGUI ScoreHeader;

    /// <summary>
    /// Content Component of the Explanations ScrollView
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public TextMeshProUGUI ExplanationTMP;

    /// <summary>
    /// ScrollView for Explanations
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public GameObject Explanations;

    /// <summary>
    /// GameObjects corresponding to the playernames and overall scores displayed on top right of the screen
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public GameObject[] PlayerCanvasEntry = new GameObject[5];

    /// <summary>
    /// GameObject corresponding to the playernames and round scores on the panel shown at the end of each round
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public GameObject[] TextPanelEntry = new GameObject[5];

    /// <summary>
    /// Canvas for the TextPanelEntry's in the result panel
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public GameObject TextPanel;

    private int roundnumber = 0;

    /// <summary>
    /// Score Overlay Canvas UI element
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public GameObject resultOverlayCanvas;

    /// <summary>
    /// Options Overlay Canvas UI element
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public GameObject OptionsOverlayCanvas;

    /// <summary>
    /// The "Beenden" Button on the result panel displayed when the game ends
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public GameObject exitButton;

    /// <summary>
    /// The "Weiter" Button at the bottom of the score overlay panel
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public GameObject continueButton;

    /// <summary>
    /// Button at the side of the score overlay panel; toggles Explanation Screen
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public GameObject explanationButton;

    /// <summary>
    /// Button at the side of the score overlay panel; toggles Score Screen
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public GameObject scoreButton;

    /// <summary>
    /// Button at the upper left corner of the screen; activates Options Screen
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public Button activateOptionsButton;
    /// <summary>
    /// Button at the bottom of the Options Screen; deactivates Options Screen
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public Button deactivateOptionsButton;
    public Boolean scoreScreenWasActive { get; set; } = false;

    /// <summary>
    /// Canvas for all round results overview
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public GameObject roundsOverview;

    /// <summary>
    /// Textfield for all round results overview
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public GameObject roundsOverview_text;

    /// <summary>
    /// Timer that is visible during a round
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public GameObject normalTimer;

    /// <summary>
    /// Timer that is visible during eval phase
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public GameObject overlayTimer;

    [Client]
    public override void OnStartLocalPlayer()
    {
        explanationButton.SetActive(true);
        exitButton.SetActive(false);
    }

    /// <summary>
    /// Updates the ScoreHeader (in ScoreResultOverlay) with given roundNumber
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    [Server]
    public void UpdateScoreHeader(int roundNumber)
    {
        roundnumber = roundNumber;
        RpcUpdateScoreHeaderText($"~ Punkte in Runde {roundNumber} ~");
    }

    /// <summary>
    /// Updates the ScoreHeader (in ScoreResultOverlay) with given text
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    [ClientRpc]
    public void RpcUpdateScoreHeaderText(string text) =>
        ScoreHeader.text = text;

    /// <summary>
    /// Updates a PlayerCanvasEntry with given index, playername and score
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
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
    /// For round point view and final point view.
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    [Server]
    public void UpdateTextPanelEntry(int idx, string player, int points, bool gameend) =>
        RpcUpdateTextPanelEntry(idx, player, points, gameend);

    /// <summary>
    /// Updates a TextPanelEntry (in ScoreResultOverlay) with given index, playername and score
    /// Shows no + before positive values.
    /// If game is'nt won already.
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    [Server]
    public void UpdateTextPanelEntry(int idx, string player, int points) =>
        RpcUpdateTextPanelEntry(idx, player, points, false);

    /// <summary>
    /// Sets and Formats the overview of all rounds for one player
    /// </summary>
    [ClientRpc]
    public void RpcSetRoundOverview(bool first, int rounds, int[] roundpoints)
    {
        var text_Component = roundsOverview_text.GetComponent<TMP_Text>();
        var formated = "";
        if (first)
        {
            text_Component.text = "";
            for (int i = 0; i < rounds; i++)
            {
                formated += "R" + (i + 1) + "\t";
            }

            formated += "\n";
        }

        for (int i = 0; i < rounds; i++)
        {
            formated += roundpoints[i] + "\t";
        }

        formated += "\n\n";

        text_Component.text += formated;
    }

    /// <summary>
    /// Updates a TextPanelEntry (in ScoreResultOverlay) with given index, playername and score.
    /// Shows a + before positive values if gameEnd is false.
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    [ClientRpc]
    private void RpcUpdateTextPanelEntry(int idx, string player, int points, bool gameEnd)
    {
        TextMeshProUGUI[] entry = TextPanelEntry[idx].GetComponentsInChildren<TextMeshProUGUI>(true);
        entry[0].text = player;
        entry[1].text = ((!gameEnd && points > 0) ? "+" : "") + points;
    }

    /// <summary>
    /// Switches between continue button (isActive = false) and exit button (isActive = true).
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    [ClientRpc]
    public void RpcToggleExit(bool isActive)
    {
        continueButton.SetActive(!isActive);
        explanationButton.SetActive(!isActive);
        scoreButton.SetActive(!isActive);
        exitButton.SetActive(isActive);
    }

    /// <summary>
    /// Shows or hides the points overview of all rounds
    /// Sets the size of the component automatically by the number of rounds
    /// </summary>
    [ClientRpc]
    public void RpcToggleRoundsOverview(bool isActive, int rounds)
    {
        if (isActive)
        {
            var rt = roundsOverview_text.GetComponent(typeof(RectTransform)) as RectTransform;
            rt.sizeDelta = new Vector2(50 + 100 * rounds, rt.rect.height);
        }

        roundsOverview.SetActive(isActive);
    }

    /// <summary>
    /// Switches between visible explanation screen and visible score screen
    /// </summary>
    public void ToggleExplanation(bool isActive)
    {
        explanationButton.SetActive(!isActive);
        scoreButton.SetActive(isActive);
        TextPanel.SetActive(!isActive);
        Explanations.SetActive(isActive);
        ScoreHeader.text = isActive ? "~ Wissenswertes ~" : "~ Punkte in Runde " + roundnumber + " ~";
    }

    /// <summary>
    /// Switches between visible explanation screen and visible score screen for every Player
    /// </summary>
    [ClientRpc]
    public void RpcToggleExplanation(bool isActive)
    {
        ToggleExplanation(isActive);
    }

    /// <summary>
    /// Updates the explanation text content in the overlay
    /// </summary>
    [ClientRpc]
    public void RpcUpdateExplanation(String explanation)
    {
        ExplanationTMP.text = explanation;
    }

    /// <summary>
    /// Switches between visible/invisible Options Screen
    /// </summary>
    public void ToggleOptions(bool isActive)
    {
        if(isActive){
            scoreScreenWasActive = resultOverlayCanvas.activeSelf;
            resultOverlayCanvas.SetActive(false);
        }
        OptionsOverlayCanvas.SetActive(isActive);
        if(!isActive){
            resultOverlayCanvas.SetActive(scoreScreenWasActive);
        }
    }

    /// <summary>
    /// Switches between visible/invisible Options Screen for every Player
    /// </summary>
    [ClientRpc]
    public void RpcToggleOptions(bool isActive)
    {
        ToggleOptions(isActive);
    }

    /// <summary>
    /// Sets ScoreScreenWasActive variable for every player
    /// </summary>
    [ClientRpc]
    public void RpcSetScoreScreenWasActive(Boolean active)
    {
        scoreScreenWasActive = active;
    }


    /// <summary>
    /// Opens the result overlay for all players.
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
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
    /// \author SWT-P_SS_20_Dixit
    [ClientRpc]
    public void RpcDeleteInputCard()
    {
        Destroy(GameObject.FindGameObjectsWithTag("InputCard")[0]);
    }

    /// <summary>
    /// Removes the question card for all players.
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    [ClientRpc]
    public void RpcDeleteQuestionCard()
    {
        Destroy(GameObject.FindGameObjectsWithTag("QuestionCard")[0]);
    }

    /// <summary>
    /// Removes all answer cards for all players.
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
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
    /// Shows the number of players who clicked a card and the name of the player who the answer on this card.
    /// Highlights the correct card.
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    [ClientRpc]
    public void RpcUpdateCard(UInt32 correctCard, GameObject currCard, int stamps)
    {
        var card = currCard.GetComponent<Card>();
        card.DisableSelectInput();
        card.ShowName();
        card.ShowStamps(stamps);

        if (card.id == correctCard)
        {
            card.HighlightCorrect();
        }
    }

    [ClientRpc]
    public void RpcShowOverlayTimer()
    {
        overlayTimer.SetActive(true);
        normalTimer.SetActive(false);
    }

    [ClientRpc]
    public void RpcShowNormalTimer()
    {
        normalTimer.SetActive(true);
        overlayTimer.SetActive(false);
    }

    [ClientRpc]
    public void RpcHideAllTimers()
    {
        overlayTimer.SetActive(false);
        normalTimer.SetActive(false);
    }
}