using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class DisplayManager : NetworkBehaviour
{
    public TextMeshProUGUI ScoreHeader;
    /// <summary>
    /// Updates a ScoreHeader (in ScoreResultOverlay) with given roundNumber
    /// </summary>
    [ClientRpc]
    public void RpcUpdateScoreHeader(int roundNumber) =>
        RpcUpdateScoreHeaderText($"~ Punkte in Runde {roundNumber} ~");

    /// <summary>
    /// Updates a ScoreHeader (in ScoreResultOverlay) with given text
    /// </summary>
    [ClientRpc]
    public void RpcUpdateScoreHeaderText(string text) =>
        ScoreHeader.text = text;

}
