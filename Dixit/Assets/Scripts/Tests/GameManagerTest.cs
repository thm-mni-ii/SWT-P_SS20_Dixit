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
        public IEnumerator RandomQuestionTest()
        {
            yield return new WaitForFixedUpdate();
            var gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
            for (int m = 0; m < 100; m++)
            {
                var randomQuestions = Utils.GetRandomQuestionIdxArray(30, gameManager.numberOfRounds);

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
        public IEnumerator GameTest()
        {
            yield return new WaitForSeconds(1f);
            gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

            gameManager.timerForGiveAnswer = 5;
            gameManager.timerToCheckResults = 3;
            gameManager.timerToChooseAnswer = 3;

            yield return CardsSpawned();
            for(var i = 1 ; i <= 3 ; i++)
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

        [Test]
        public void NumberToWordTest()
        {   
            Assert.AreEqual(gameManager.numberToGermanWord(0), "null");
            Assert.AreEqual(gameManager.numberToGermanWord(1), "eins");
            Assert.AreEqual(gameManager.numberToGermanWord(9), "neun");
            Assert.AreEqual(gameManager.numberToGermanWord(10), "zehn");
            Assert.AreEqual(gameManager.numberToGermanWord(11), "elf");
            Assert.AreEqual(gameManager.numberToGermanWord(12), "zwölf");
            Assert.AreEqual(gameManager.numberToGermanWord(13), "dreizehn");
            Assert.AreEqual(gameManager.numberToGermanWord(16), "sechzehn");
            Assert.AreEqual(gameManager.numberToGermanWord(19), "neunzehn");
            Assert.AreEqual(gameManager.numberToGermanWord(20), "zwanzig");
            Assert.AreEqual(gameManager.numberToGermanWord(21), "einundzwanzig");
            Assert.AreEqual(gameManager.numberToGermanWord(32), "zweiunddreißig");
            Assert.AreEqual(gameManager.numberToGermanWord(40), "vierzig");
            Assert.AreEqual(gameManager.numberToGermanWord(67), "siebenundsechzig");
            Assert.AreEqual(gameManager.numberToGermanWord(75), "fünfundsiebzig");
            Assert.AreEqual(gameManager.numberToGermanWord(99), "neunundneunzig");
            Assert.AreEqual(gameManager.numberToGermanWord(100), "einhundert");
            Assert.AreEqual(gameManager.numberToGermanWord(101), "einhunderteins");
            Assert.AreEqual(gameManager.numberToGermanWord(234), "zweihundertvierunddreißig");
            Assert.AreEqual(gameManager.numberToGermanWord(999), "neunhundertneunundneunzig");
        }

        [Test]
        public void EqualityTest()
        {
            Assert.IsTrue(gameManager.AnswersAreEqual("aussage", "Aussage"));

            Assert.IsTrue(gameManager.AnswersAreEqual("8 Bit", "acht bit"));

            Assert.IsTrue(gameManager.AnswersAreEqual("Es sind 3", "Es sind drei"));

            Assert.IsTrue(gameManager.AnswersAreEqual("fünfunddreißig grad", "35 grad"));

            Assert.IsTrue(gameManager.AnswersAreEqual("Beinhaltet 18 Liter", "beinhaltet achtzehn Liter"));
        }
    }
}
