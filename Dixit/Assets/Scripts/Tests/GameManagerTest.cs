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
