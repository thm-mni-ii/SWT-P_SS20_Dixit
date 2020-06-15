/* created by: SWT-P_SS_20_Dixit */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

/// <summary>
/// Represents a Player in the Game.
/// Communicates with the GameManager to log Answers and change the current Game Phase.
/// </summary>
public class Player : NetworkBehaviour
{
    private int points { get; set; }
    private string playerName { get; set; }
    public GameObject m_cardPrefab;

    public GameManager gameManager;

    /// <summary>
    /// Called when the local Player Object has been set up
    /// </summary>
    public override void OnStartServer()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    /// <summary>
    /// Sends the given Answer from a Player to the GameManager.
    /// <param name="answer">The Answer.</param>
    /// </summary>
    [Command]
    public void CmdGiveAnswer(string answer)
    {
        gameManager.LogAnswer(this.netIdentity, answer);
    }

    /// <summary>
    /// Sends the choosen Answer from a Player to the GameManager
    /// <param name="answer">The Answer</param>
    /// </summary>
    [Command]
    public void CmdChooseAnswer(int answer)
    {
        gameManager.LogAnswer(this.netIdentity, answer);
    }

    [ClientRpc]
    public void RpcRenderAnswers(string[] answers){
        
        var startX = (answers.Length * 125 + (answers.Length -1) * 20) / 2;

        for(int i = 0; i < answers.Length; i++)
        {

            var xPosition = startX -  62.5 - i * 145;

            var card = (GameObject) Instantiate (m_cardPrefab, new Vector3( (float)  xPosition , -100, -2), Quaternion.Euler(0,0,0));
            card.GetComponentInChildren<Transform>().Find("WriteAnswer").gameObject.SetActive(false);
            card.GetComponentInChildren<Transform>().Find("SelectAnswer").gameObject.SetActive(true);


            card.GetComponentInChildren<Transform>().Find("SelectAnswer").gameObject.GetComponentInChildren<Transform>()
                .Find("Text (TMP)").GetComponent<TMPro.TMP_Text>().text = answers[i];
                
            card.gameObject.SetActive(true);
        }
        

        

    }

}
