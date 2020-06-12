/* created by: SWT-P_SS_20_Dixit */
using System;
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
    private Lazy<Player> _localPlayer = new Lazy<Player>(() => ClientScene.localPlayer.gameObject.GetComponent<Player>());
    public Player LocalPlayer => _localPlayer.Value;

    /// <summary>
    /// Calls the GiveAnswer Command on the local Player.
    /// <param name="answer">The given Answer.</param>
    /// </summary>
    public void GiveAnswer(string answer)
    {
        LocalPlayer.CmdGiveAnswer(answer);
    }

    public void GiveAnswer(TMPro.TMP_InputField answer){
        GiveAnswer(answer.text);
    }

    public void SelectAnswer()
    {
        LocalPlayer.CmdChooseAnswer(1);
    }
}
