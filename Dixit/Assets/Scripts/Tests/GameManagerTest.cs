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
        private GameManager gameManager;

        [SetUp]
        public void Setup()
        {
            GameObject gameManagerGameObject = MonoBehaviour.Instantiate(Resources.Load("Prefabs/GameManager")) as GameObject;
            gameManager = gameManagerGameObject.GetComponent<GameManager>();
        }

        [TearDown]
        public void Teardown()
        {
            Object.Destroy(gameManager.gameObject);
        }

        [Test]
        public void RandomQuestionTest()
        {
            for (int m = 0; m < 100; m++)
            {
                var randomQuestions = gameManager.GetRandomQuestionIdxArray(30);

                for (int i = 0; i < randomQuestions.Length; i++)
                {
                    for (int k = 0; k < i; k++)
                    {
                        Assert.AreNotEqual(randomQuestions[k], randomQuestions[i]);
                    }
                }
            }
        }
    }
}
