
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
        private bool serverStarted = false;

        [SetUp]
        public void Setup()
        {
            if(!serverStarted)
            {
                GameObject gso = MonoBehaviour.Instantiate(Resources.Load("Prefabs/NetworkManager")) as GameObject;
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
        public IEnumerator HostPlayerAdded()
        {
            yield return new WaitForFixedUpdate();
            Assert.AreEqual(1, NetworkServer.connections.Count, "No connection added");
            Assert.AreEqual(1, gameServer.numPlayers, "No player added");
        }

        [UnityTest]
        public IEnumerator ClientPlayerAdded()
        {
            yield return new WaitForSeconds(2f);
            Assert.AreEqual(2, gameServer.numPlayers, "No player added");
            Assert.AreEqual(2, NetworkServer.connections.Count, "No connection added");
        }

    }
}