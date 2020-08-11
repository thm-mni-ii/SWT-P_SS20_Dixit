using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Mirror;

namespace Tests
{
    public class GameManagerTest
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

                gameServer.OnClientConnect(new NetworkConnectionToClient(2));
            }
        }

        [UnityTest]
        public IEnumerator RandomQuestionTest()
        {
            yield return new WaitForFixedUpdate();
            var gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
            for (int m = 0; m < 100; m++)
            {
                var randomQuestions = gameManager.GetRandomQuestionIdxArray(30);

                for (int i = 0; i < randomQuestions.Length; i++)
                {
                    for (int k = 0; k < i; k++)
                    {
                        Assert.AreNotEqual(randomQuestions[k], randomQuestions[i], "Array not unique");
                    }
                }
            }
        }

        [UnityTest]
        public IEnumerator CardsSpawned()
        {
            yield return new WaitForSeconds(2f);
            Assert.NotNull(GameObject.FindGameObjectWithTag("QuestionCard"), "No QuestionCard spawned");
            Assert.NotNull(GameObject.FindGameObjectWithTag("InputCard"), "No InputCard spawned");
        }
    }
}
