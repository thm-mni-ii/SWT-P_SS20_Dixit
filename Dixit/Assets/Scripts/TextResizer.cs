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
/// Script for resizing otherwise independet text groups to be identical in size
/// \author SWT-P_SS_20_Dixit
/// </summary>
public class TextResizer : NetworkBehaviour
{

    [SerializeField] private TextMeshProUGUI[] TextGroup;
    private float smallestFontSize;

    /// <summary>
    /// Searches for smallest fontsize in text group and sets all members fontsize to that value (executed for every Player)
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    [ClientRpc]
    public void RpcAssimilateSize()
    {
        foreach(TextMeshProUGUI text in TextGroup)
        {
            smallestFontSize = text.fontSize < smallestFontSize ? text.fontSize : smallestFontSize;
        }

        foreach(TextMeshProUGUI text in TextGroup)
        {
            text.fontSize = smallestFontSize;
        }

    }

    /// <summary>
    /// Switches between enabled/disabled autosize for every member of the text group (executed for every Player)
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    [ClientRpc]
    public void RpcToggleAutoSize(Boolean activate){
        foreach(TextMeshProUGUI text in TextGroup)
        {
            text.enableAutoSizing = activate;
        }
    }

}
