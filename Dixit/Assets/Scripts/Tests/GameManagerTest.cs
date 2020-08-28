using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Mirror;

namespace Tests
{
    public class GameManagerTest
    {
        public GameServer gameServer;
        public GameManager gameManager;
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
        public IEnumerator GameTest()
        {
            yield return new WaitForSeconds(1f);
            gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

            gameManager.timerForGiveAnswer = 5;
            gameManager.timerToCheckResults = 3;
            gameManager.timerToChooseAnswer = 3;

            yield return CardsSpawned();
            for(var i = 1; i <= 3; i++)
            {
                yield return AnswersGiven(i);
                yield return AnswersChosen(i);
            }
        }

        private IEnumerator CardsSpawned()
        {
            var conn = new NetworkConnectionToClient(2);
            NetworkServer.AddConnection(conn);
            gameServer.OnServerAddPlayer(conn);

            yield return new WaitForSeconds(1f);

            foreach((var player, var idx) in Utils.GetPlayersIndexed())
            {
                if(player.PlayerName == null || player.PlayerName =="")
                {   
                    player.PlayerName = "Mustermann" + idx;
                    player.CmdSendName( "Mustermann" + idx);
                }
            }   

            yield return new WaitForSeconds(1f);
            
            Assert.NotNull(GameObject.FindGameObjectWithTag("QuestionCard"), "No QuestionCard spawned");
            Assert.NotNull(GameObject.FindGameObjectWithTag("InputCard"), "No InputCard spawned");
            yield return null;
        }

        private IEnumerator AnswersGiven(int round)
        {
        yield return new WaitForSeconds(1f);

            switch (round)
            {
                case 1:
                {
                    foreach((Player p, int i ) in Utils.GetPlayersIndexed())
                    {
                        gameManager.LogAnswer(p.netId, $"Test {i}");
                    }
                    break;
                }
                case 2:
                {
                    foreach(Player p in Utils.GetPlayers())
                    {
                        gameManager.LogAnswer(p.netId, "Test");
                    }
                    break;
                }
                case 3:
                {
                    gameManager.LogAnswer(Utils.GetPlayers().First().netId, "");
                    break;
                }
                default:
                    break;
            }

            yield return new WaitForSeconds(2f);
           
           switch (round)
           {
                case 1:
                    Assert.AreEqual(3, GameObject.FindGameObjectsWithTag("AnswerCard").Length, "Wrong amount of possible answers");
                    break;
                case 2:
                    yield return new WaitForSeconds(gameManager.timerForGiveAnswer - 2);
                    break;
                default:
                    break;
           }
            
            yield return null;
        }

        private IEnumerator AnswersChosen(int round)
        {
            var players = Utils.GetPlayers().ToArray();
            switch (round)
            {
                case 1:
                {
                    gameManager.LogAnswer(players[0].netId, gameManager.netId);
                    gameManager.LogAnswer(players[1].netId, players[0].netId);
                    break;
                }
                default:
                    break;
            }

            var timer = GameObject.Find("TimerController").GetComponent<CountdownTimer>();
            yield return new WaitForSeconds(gameManager.timerToChooseAnswer);

            switch (round)
            {
                case 1:
                {
                    Assert.AreEqual(4, gameManager.points[players[0].netId], "Correct answer + other player clicked on answer wrongly calculated");
                    Assert.AreEqual(0, gameManager.points[players[1].netId], "No points should have been awarded");
                    break;
                }
                case 2:
                {
                    Assert.AreEqual(4, gameManager.points[players[0].netId], "No points should be awarded");
                    Assert.AreEqual(0, gameManager.points[players[1].netId], "No points should be awarded");
                    break;
                }
                case 3:
                    Assert.AreEqual(3, gameManager.points[players[0].netId], "Empty answer should result in -1 point");
                    Assert.AreEqual(-1, gameManager.points[players[1].netId], "No answer should result in -1 point");
                    break;
                default:
                    break;
            }

            foreach(var p in Utils.GetPlayers())
            {
                gameManager.LogPlayerIsReady();
            }
            yield return new WaitForSeconds(gameManager.timerToCheckResults);
        }
    }
}
