
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Mirror;

namespace Tests
{
    public class CardTest
    {
        public GameObject questionGo;
        public GameObject inputGo;

        public Card card;
      
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


        private IEnumerator CardsSpawned()
        {
            var conn = new NetworkConnectionToClient(2);
            NetworkServer.AddConnection(conn);
            gameServer.OnServerAddPlayer(conn);
            yield return new WaitForSeconds(1f);

            questionGo = GameObject.FindGameObjectWithTag("QuestionCard");
            inputGo = GameObject.FindGameObjectWithTag("InputCard");

            Assert.NotNull(questionGo, "No QuestionCard spawned");
            Assert.NotNull(inputGo, "No InputCard spawned");
            yield return null;
        }

        [UnityTest]
        public IEnumerator FlipTest()
        {
            yield return new WaitForSeconds(1f);
            yield return CardsSpawned();
           
            inputGo.GetComponent<Card>().InstantFlipFacedown();
            yield return new WaitForSeconds(1f);
            Assert.AreEqual(Quaternion.Euler(0, 180, 0).ToString(), inputGo.transform.GetChild(0).transform.rotation.ToString());

            inputGo.GetComponent<Card>().InstantFlipFaceup();
            yield return new WaitForSeconds(1f);
            Assert.AreEqual(Quaternion.Euler(0, 0, 0).ToString(), inputGo.transform.GetChild(0).transform.rotation.ToString());
           
            inputGo.GetComponent<Card>().FlipFacedown();
            yield return new WaitForSeconds(3f);
            Assert.AreEqual(Quaternion.Euler(0, 180, 0).ToString(), inputGo.transform.GetChild(0).transform.rotation.ToString());

            inputGo.GetComponent<Card>().FlipFaceup();
            yield return new WaitForSeconds(3f);
            Assert.AreEqual((new Quaternion(0,0,0,-1)).ToString(), inputGo.transform.GetChild(0).transform.rotation.ToString());  

            inputGo.GetComponent<Card>().FlipFacedown(2f);
            yield return new WaitForSeconds(1.5f);
            Assert.AreEqual((new Quaternion(0,0,0,-1)).ToString(), inputGo.transform.GetChild(0).transform.rotation.ToString());
            yield return new WaitForSeconds(1f);
            Assert.AreEqual((new Quaternion(0,1,0,0)).ToString(), inputGo.transform.GetChild(0).transform.rotation.ToString());
            
            inputGo.GetComponent<Card>().FlipFaceup(2f);
            yield return new WaitForSeconds(1.5f);
            Assert.AreEqual((new Quaternion(0,1,0,0)).ToString(), inputGo.transform.GetChild(0).transform.rotation.ToString());
            yield return new WaitForSeconds(1f);
            Assert.AreEqual((new Quaternion(0,0,0,-1)).ToString(), inputGo.transform.GetChild(0).transform.rotation.ToString());    
        }

    }
}