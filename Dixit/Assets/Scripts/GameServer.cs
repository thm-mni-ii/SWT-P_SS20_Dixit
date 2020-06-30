using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.Websocket;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// This class represents a game server.
/// The server starts automatically and loads the player prefab into the online scene.
/// </summary>
public class GameServer : NetworkManager
{
    /// <summary>
    /// Location of the file that holds playerInfo of
    /// the player that started the game.
    /// NOTE: This is not important for development.
    /// </summary>
    private const string FILE_NAME = "PlayerInfo.txt";

    /// <summary>
    /// Stores the information of the player that started the game.
    /// </summary>
    private PlayerInfo localPlayerInfo;

    /// <summary>
    /// Defines how long the game server will try to connect the client before quitting the application.
    /// </summary>
    [SerializeField] private float disconnectWaitTime;

    /// <summary>
    /// Stores the time until disconnect.
    /// </summary>
    private float disconnectTimer;
    
    public PlayerInfo LocalPlayerInfo => localPlayerInfo;
    public static GameServer Instance => (GameServer) singleton;

    public int playersWantToPlay = 2;

    /// <summary>
    /// Start is called before the first frame update.
    /// The server loads the player info of the local player, if:
    /// - the player is the host -> start server as host
    /// - the player is the client -> join server as client
    /// NOTE: This is not important for development.
    /// </summary>
    public override void Start()
    {
        base.Start();

        LoadPlayerInfoMockup();

        /*LoadPlayerInfo();

        if (localPlayerInfo.isHost)
        {
            StartHost();
        }
        else
        {
            disconnectTimer = disconnectWaitTime;
        }*/

    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
       Transform startPos = GetStartPosition();
        GameObject player = startPos != null
            ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
            : Instantiate(playerPrefab);

        Player p = player.GetComponent<Player>();
        p.PlayerName = localPlayerInfo.name;

        NetworkServer.AddPlayerForConnection(conn, player);
        
        if(numPlayers == playersWantToPlay)
        {
            GameManager gameManager = GameObject.Find("GameManager").GetComponent<GameManager>(); 
            gameManager.StartGame();       
        }
    }
    /// <summary>
    /// Update is called once per frame.
    /// Checks if the local player is a client and not connected.
    /// If so the client tries to connect and the disconnect timer is updated.
    /// </summary>
    private void Update()
    {
        if(!localPlayerInfo.isHost && !NetworkClient.isConnected)
        {
            disconnectTimer -= Time.deltaTime;

            if (disconnectTimer <= 0f)
            {
                Application.Quit();
            }
            
            StartClient();
        }
    }

    /// <summary>
    /// Loads player info from file.
    /// NOTE: This is not important for development.
    /// </summary>
    private void LoadPlayerInfo()
    {
        StreamReader file = new StreamReader(Application.dataPath + @"\..\..\..\Framework\" + FILE_NAME);
        localPlayerInfo = (PlayerInfo)JsonUtility.FromJson(file.ReadLine(),typeof(PlayerInfo));
        file.Close();
        File.Delete(FILE_NAME);
    }
    
    /// <summary>
    /// Sets mockup player info.
    /// The actual info is irrelevant. It does NOT matter if the info says the player is host.
    /// This will become relevant when using loadPlayerInfo.
    /// NOTE: This is important for development.
    /// </summary>
    private void LoadPlayerInfoMockup()
    {
        localPlayerInfo = new PlayerInfo("Mustermann", true);
    }

}
