/* created by: SWT-P_SS_20_Dixit */
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

public class TextResizer : NetworkBehaviour
{

    [SerializeField] private TextMeshProUGUI[] TextGroup;
    private float smallestFontSize;

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

    [ClientRpc]
    public void RpcToggleAutoSize(Boolean activate){
        foreach(TextMeshProUGUI text in TextGroup)
        {
            text.enableAutoSizing = activate;
        }
    }

}
