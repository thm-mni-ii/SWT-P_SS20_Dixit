/* created by: SWT-P_SS_20_Dixit */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

/// <summary>
/// Handles Player input.
/// </summary>
/// \author SWT-P_SS_20_Dixit
public class PlayerInput : MonoBehaviour
{
    /// <summary>
    /// The "Beenden" Button on the result panel displayed when the game ends
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public Button exitButton;

    /// <summary>
    /// The "Weiter" Button at the bottom of the score overlay panel
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public Button continueButton;

    /// <summary>
    /// The <c>DisplayManager</c>
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public DisplayManager displayManager;

    /// <summary>
    /// The singelton of the Player Input
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public static PlayerInput singleton {get ; private set ;}

    /// <summary>
    /// Is set true when the player is allowed to submit an answer
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public bool canSubmit {get ; set ;} = false;
    
    private bool optionsOn = false;

    void Start()
    {
        if(singleton != null)
        {
            Debug.LogError("Multible PlayerInputs detected!"); 
            Destroy(gameObject);
        }
        else
        {
            singleton = this;
        }
    }

    void Update()
    {
        if(canSubmit && Input.GetButtonUp("Submit"))
        {
            var card = GameObject.FindGameObjectWithTag("InputCard");
            card.GetComponent<Card>().FlipFacedown();
            GiveAnswer(card.GetComponentInChildren<TMP_InputField>().text);
        }

        if(Input.GetButtonUp("Cancel"))
        {
            ToggleOptions();
        }
    }
    
    /// <summary>
    /// Sends the answer the player gave to the server
    /// </summary>
    /// <param name="answer">The given Answer.</param>
    /// \author SWT-P_SS_20_Dixit
    public void GiveAnswer(string answer)
    {
        Player.LocalPlayer.GiveAnswer(answer);
    }

    /// <summary>
    /// Signals that the player clicked on the "Weiter" button
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public void ClickContinueButton()
    {
        continueButton.interactable = false;
        Player.LocalPlayer.CmdPlayerIsReady();
    }

    /// <summary>
    /// Signals that the player clicked on the "Beenden" Button
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public void EndGame()
    {
       exitButton.interactable = false;
       Player.LocalPlayer.CmdFinishGame();
    }

    /// <summary>
    /// Switches between visible explanation screen and visible score screen
    /// </summary>
    public void ToggleExplanation(bool isActive)
    {
        displayManager.ToggleExplanation(isActive);
    }

    /// <summary>
    /// Switches between visible/invisible Options Screen
    /// </summary>
    public void ToggleOptions()
    {
        displayManager.ToggleOptions(optionsOn = !optionsOn);
    }

}
