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
    private const string FILE_NAME = "MyFile.txt";

    /// <summary>
    /// Stores the information of the player that started the game.
    /// </summary>
    private PlayerInfo localPlayerInfo;

    public PlayerInfo LocalPlayerInfo => localPlayerInfo;
    
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
            StartClient();
        }
        */
    }

    /// <summary>
    /// Loads player info from file.
    /// NOTE: This is not important for development.
    /// </summary>
    private void LoadPlayerInfo()
    {
        StreamReader file = new StreamReader(Application.dataPath + @"\..\..\..\" + FILE_NAME);
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

/*
    public GameManager gameManager {get; private set;}

    public override void OnServerSceneChanged(string scene) { 
        var gm = UnityEngine.Object.Instantiate(spawnPrefabs[0]);
        NetworkServer.Spawn(gm);
        gameManager = gm.GetComponent<GameManager>();
        Debug.Log("wurde aufgerufen! Szene: "+scene);
    }*/
}
