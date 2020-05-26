﻿/* created by: SWT-P_SS_20_Dixit */
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;

/// <summary>
/// The GameManger keeps track of all given Answers and Changes the curren Phase of the Game.
/// It only exists on the Server.
/// </summary>
public class GameManager : NetworkBehaviour
{
    private Dictionary<NetworkIdentity, string> answers = new Dictionary<NetworkIdentity, string>();
    private Dictionary<NetworkIdentity, int> choices = new Dictionary<NetworkIdentity, int>();
    
    private NetworkManager networkManager;
    
    private enum Phase { WriteAnswer, ChoseAnswer }
    private Phase currentPhase;

    /// <summary>
    /// Called when the GameManger starts on the Server.
    /// </summary>
    public override void OnStartServer()
    {
        networkManager = NetworkManager.singleton;

        currentPhase = Phase.WriteAnswer;
    }

    /// <summary>
    /// Gets the list of Players in the current Game.
    /// <returns>The list of players.</returns>
    /// </summary>
    private List<Player> GetPlayers()
    {
        return NetworkServer.connections.Values.Select(c => c.identity.gameObject.GetComponent<Player>()).ToList();
    }


    /// <summary>
    /// Logs the given Answer of a Player during the WriteAnwer Phase.
    /// </summary>
    public void LogAnswer(NetworkIdentity player, string answer)
    {
        answers.Add(player, answer);

        if (answers.Count == networkManager.numPlayers)
        {
            ChangePhase();
        }
    }

    /// <summary>
    /// Logs the choosen Answer of a Player during the ChoseAnswer Phase.
    /// </summary>
    public void LogAnswer(NetworkIdentity player, int choice)
    {
        choices.Add(player, choice);
        if (answers.Count == networkManager.numPlayers)
        {
            ChangePhase();
        }
    }

    /// <summary>
    /// Changes Phase in the GameManager and calls the Corresponding RPC-Call
    /// </summary>
    private void ChangePhase()
    {
        switch (currentPhase)
        {
            case Phase.WriteAnswer:
                currentPhase = Phase.ChoseAnswer;
                RpcChangePhase();
                break;
            case Phase.ChoseAnswer:
                currentPhase = Phase.WriteAnswer;
                RpcChangePhase();
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Changes the Scene on every Client to the correct Phase
    /// </summary>
    [ClientRpc]
    public void RpcChangePhase()
    {
        foreach (var a in GetPlayers())
        {
           Debug.Log(a.netId);
        }
    }
}