/* created by: SWT-P_SS_20_Dixit */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

/// <summary>
/// Handles Player input.
/// </summary>
public class PlayerInput : MonoBehaviour
{

    /// <summary>
    /// Calls the GiveAnswer Command on the local Player.
    /// <param name="answer">The given Answer.</param>
    /// </summary>
    public void GiveAnswer(string answer)
    {
        Player.LocalPlayer.CmdGiveAnswer(answer);
    }

    public void GiveAnswer(TMPro.TMP_InputField answer){
        GiveAnswer(answer.text);
    }

    public void SelectAnswer()
    {
        var card = GetComponentInParent<Card>();
        Player.LocalPlayer.CmdChooseAnswer(card.choosen);
    }
}
