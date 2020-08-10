﻿/* created by: SWT-P_SS_20_Dixit */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

/// <summary>
/// Handles Player input.
/// </summary>
/// \author SWT-P_SS_20_Dixit
public class PlayerInput : MonoBehaviour
{

    /// <summary>
    /// Sends the answer the player gave to the server
    /// <param name="answer">The given Answer.</param>
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public void GiveAnswer(string answer)
    {
        Player.LocalPlayer.CmdGiveAnswer(answer);
    }

    /// <summary>
    /// Sends the answer the player gave to the server
    /// <param name="answer">The text field the answer was written in.</param>
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public void GiveAnswer(TMPro.TMP_InputField answer){
        GiveAnswer(answer.text);
    }

    /// <summary>
    /// Sends the id of the card the player chose to the server
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public void SelectAnswer()
    {
        var card = GetComponentInParent<Card>();
        Player.LocalPlayer.ChooseAnswer(card);
    }

    /// <summary>
    /// Signals that the player clicked on the "Weiter" button
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public void ClickContinueButton()
    {
        this.GetComponent<Button>().interactable = false;
        Player.LocalPlayer.CmdPlayerIsReady();
    }

    /// <summary>
    /// Signals that the player clicked on the "Beenden" Button
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public void EndGame()
    {
       this.GetComponent<Button>().interactable = false;
       Player.LocalPlayer.KillGame(); 
    }

    /// <summary>
    /// Signals that the player clicked on the "restart" button
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public void Restart()
    {
       this.GetComponent<Button>().interactable = false;
       Player.LocalPlayer.CmdRestart(); 
    }
}
