using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Mirror;
using System.Threading.Tasks;
using Firebase.Extensions;


namespace Tests
{
    public class DatabaseTest
    {
        private Task<QuestionSet> loadQuestionSet;
        public QuestionSet QuestionSet => loadQuestionSet.Result;
        public GameServer gameServer;
        public GameObject gameManager;
        private bool serverStarted = false;

        [UnitySetUp]
        public IEnumerator Setup()
        {
            if(!serverStarted)
            {
                var gso = MonoBehaviour.Instantiate(Resources.Load("Prefabs/NetworkManager")) as GameObject;
                gameServer = gso.GetComponent<GameServer>();
                gameServer.StartHost();
                serverStarted = true;
            }

            yield return new WaitForSeconds(1f);

            gameManager = GameObject.Find("GameManager");
            
            loadQuestionSet = QuestionSet.RetrieveQuestionSet("0", gameManager.GetComponent<DatabaseSetup>().DB)
            .ContinueWithLogException();
        }

        [Test]
        public void LoadTest()
        {
            //wait until the question set is loaded
            loadQuestionSet.ContinueWithOnMainThread(t =>
            {
                
            });
        }

        public void ReadQuestion()
        {
           QuestionSet.GetQuestion(0).ContinueWithLogException().ContinueWithOnMainThread(
            l =>
            {
                Assert.NotNull(l.Result);

                Assert.AreNotEqual("", l.Result.QuestionText);
                Assert.AreNotEqual("", l.Result.Answer);
            });


        }

      
    }
}
