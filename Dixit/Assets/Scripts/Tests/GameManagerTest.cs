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

    }
}
