using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;

public class GameManager : NetworkBehaviour
{
    private Dictionary<NetworkIdentity, string> answers = new Dictionary<NetworkIdentity, string>();
    private Dictionary<NetworkIdentity, int> choices = new Dictionary<NetworkIdentity, int>();
    
    private NetworkManager networkManager;
    
    private enum Phase { WriteAnswer, ChoseAnswer }
    private Phase currentPhase;

    public void Start()
    {
        networkManager = NetworkManager.singleton;

        currentPhase = Phase.WriteAnswer;
    }

    public List<Player> GetPlayers()
    {
        return NetworkServer.connections.Values.Select(c => c.identity.gameObject.GetComponent<Player>()).ToList();
    }


    public void LogAnswer(NetworkIdentity player, string answer)
    {
        answers.Add(player, answer);

        if (answers.Count == networkManager.numPlayers)
        {
            ChangePhase();
        }
    }

    public void LogAnswer(NetworkIdentity player, int choice)
    {
        choices.Add(player, choice);
        if (answers.Count == networkManager.numPlayers)
        {
            ChangePhase();
        }
    }

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
