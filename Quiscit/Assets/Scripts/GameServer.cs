using System;
using System.Collections;
using System.IO;
using Mirror;
using SimpleJSON;
using UnityEngine;


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
    private const string FILE_NAME = "Info.txt";

    public static bool TEST_MODE = false;

    /// <summary>
    /// Defines how long the game server will try to connect the client before quitting the application.
    /// </summary>
    [SerializeField]
    private float disconnectWaitTime;

    /// <summary>
    /// Stores the time until disconnect.
    /// </summary>
    private float disconnectTimer;

    /// <summary>
    /// Stores if the local player is the host.
    /// </summary>
    private bool isHost;

    /// <summary>
    /// Signals if the game can be quit.
    /// </summary>
    private bool readyToQuit;

    /// <summary>
    /// Stores whether the client ties to connect at the moment.
    /// </summary>
    private bool tryingToConnect;

    /// <summary>
    /// Stores information about the player.
    /// </summary>
    public PlayerInfo PlayerInfos { get; private set; }

    /// <summary>
    /// Stores information about the game.
    /// </summary>
    public JSONNode GameInfos { get; private set; }

    /// <summary>
    /// The <c>GameManager</c>
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public GameManager GameManager;

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

        readyToQuit = false;
        isHost = false;

        if (TEST_MODE)
        {
            LoadPlayerInfoMockup();
        }
        else
        {
            LoadPlayerInfo();

            if (isHost)
                StartHost();
            else
                disconnectTimer = disconnectWaitTime;
        }
    }


    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        Transform startPos = GetStartPosition();
        GameObject player = startPos != null
            ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
            : Instantiate(playerPrefab);

        NetworkServer.AddPlayerForConnection(conn, player);

        if (numPlayers == GameInfos["playerAmount"])
            StartCoroutine(WaitAndStart());
    }

    private IEnumerator WaitAndStart()
    {
        foreach (var p in Utils.GetPlayers())
            while (p.PlayerName == null || p.PlayerName == "")
                yield return new WaitForSeconds(0.5f);

        GameManager gameManager = GameManager.Instance;

        gameManager.questionSetID = GameInfos["Question Set"];
        gameManager.numberOfRounds = GameInfos["Rounds"];
        gameManager.TimerForGiveAnswer = GameInfos["Answer Time"];
        gameManager.TimerToChooseAnswer = GameInfos["Picking Time"];

        gameManager.StartGame();
    }

    /// <summary>
    /// Update is called once per frame.
    /// Checks if the local player is a client and not connected.
    /// If so the client tries to connect and the disconnect timer is updated.
    /// </summary>
    private void Update()
    {
        // Try connecting if not host
        if (!isHost && !NetworkClient.isConnected && !tryingToConnect)
        {
            tryingToConnect = true;

            disconnectTimer -= Time.deltaTime;

            if (disconnectTimer <= 0f)
                Application.Quit();

            StartClient();
        }

        // Try quitting
        if (readyToQuit)
        {
            if (isHost)
            {
                // Host waits for all players to quit first
                if (NetworkServer.connections.Count <= 1)
                    Application.Quit();
            }
            else
            {
                Application.Quit();
            }
        }
    }

    /// <summary>
    /// Loads player info from file.
    /// NOTE: This is not important for development.
    /// </summary>
    private void LoadPlayerInfo()
    {
        // Read file
        string filePath;
        switch (SystemInfo.operatingSystemFamily)
        {
            case OperatingSystemFamily.Windows:
                filePath = Application.dataPath + @"\..\..\..\..\Framework\Windows\" + FILE_NAME;
                break;
            case OperatingSystemFamily.Linux:
                filePath = Application.dataPath + "/../../../../Framework/Linux/" + FILE_NAME;
                break;
            case OperatingSystemFamily.MacOSX:
                filePath = Application.dataPath + "/../../../../Framework/MACOSX/" + FILE_NAME;
                break;
            default:
                throw new ArgumentException("Illegal OS !");
        }

        StreamReader file = new StreamReader(filePath);
        JSONNode jsonFile = JSON.Parse(file.ReadLine());

        // Load data
        isHost = jsonFile["playerInfo"]["isHost"].AsBool;
        PlayerInfos = new PlayerInfo(jsonFile["playerInfo"]["name"], isHost);
        GameInfos = jsonFile["gameInfo"];

        // Close file
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
        isHost = true;
        PlayerInfos = new PlayerInfo("Mustermann", isHost);

        GameInfos = new JSONObject();
        GameInfos.Add("Question Set", "1");
        GameInfos.Add("Rounds", "3");
        GameInfos.Add("Answer Time", "15");
        GameInfos.Add("Picking Time", "15");
        GameInfos.Add("playerAmount", "2");
    }

    /// <summary>
    /// Handles the results of the game and sets the readyToQuit flag.
    /// Needs the placement of the local player and the name of the winning player
    /// and writes them into a JSON that is used to give rewards in the framework.
    /// IMPORTANT: THIS HAS TO BE CALLED AT THE END OF THE GAME!
    /// </summary>
    /// <param name="localPlayerWinningPlacement">Placement of the local player</param>
    /// <param name="nameOfWinner">Name of the winner</param>
    public void HandleGameResults(int localPlayerWinningPlacement, string nameOfWinner)
    {
        // Create file
        if (File.Exists(FILE_NAME))
            File.Delete(FILE_NAME);

        var sr = File.CreateText(FILE_NAME);

        // Write file
        JSONObject fileJson = new JSONObject();

        fileJson.Add("placement", localPlayerWinningPlacement);
        fileJson.Add("nameOfWinner", nameOfWinner);

        sr.Write(fileJson.ToString());
        sr.Close();

        readyToQuit = true;
    }

    public override void OnStartClient()
    {
        GetComponent<NetworkManagerHUD>().enabled = false;
    }

}
