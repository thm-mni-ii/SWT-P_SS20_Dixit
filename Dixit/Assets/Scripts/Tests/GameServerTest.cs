
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Mirror;

namespace Tests
{
    public class GameServerTest
    {
        public GameServer gameServer;
        public GameManager gameManager;
        private bool serverStarted = false;

        [SetUp]
        public void Setup()
        {
            if(!serverStarted)
            {
                var gso = MonoBehaviour.Instantiate(Resources.Load("Prefabs/NetworkManager")) as GameObject;
                gameServer = gso.GetComponent<GameServer>();
                gameServer.StartHost();
                serverStarted = true;
            }
        }

        [UnityTest]
        public IEnumerator ServerExists()
        {
           Assert.NotNull(NetworkManager.singleton, "No server enabled");
           yield return null;
        }

        [UnityTest]
        public IEnumerator HostActive()
        {
            Assert.True(gameServer.isNetworkActive, "No host started");
            yield return null;
        }

        [UnityTest]
        public IEnumerator ClientPlayerAdded()
        {
            yield return new WaitForSeconds(1f);
            var conn = new NetworkConnectionToClient(2);
            NetworkServer.AddConnection(conn);
            //var p = MonoBehaviour.Instantiate(Resources.Load("Prefabs/Player")) as GameObject;
            gameServer.OnServerAddPlayer(conn);
            yield return new WaitForFixedUpdate();
            Assert.AreEqual(2, NetworkServer.connections.Count, "No connection added");
            Assert.AreEqual(2, gameServer.numPlayers, "No player added");

        }
    }
}