
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
            yield return CardsSpawned();
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

            questionGo = GameObject.FindGameObjectWithTag("QuestionCard");
            inputGo = GameObject.FindGameObjectWithTag("InputCard");

            Assert.NotNull(questionGo, "No QuestionCard spawned");
            Assert.NotNull(inputGo, "No InputCard spawned");
            yield return null;
        }

        [UnityTest]
        public IEnumerator FlipTest()
        {
           
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

        [UnityTest]
        public IEnumerator SlideTest()
        {
            questionGo.GetComponent<Card>().RpcSlideToPosition(new Vector3(100,0,-1));
            yield return new WaitForSeconds(3f);

            Assert.AreEqual((new Vector3(100,0,-1)).ToString(), questionGo.transform.position.ToString());

            questionGo.GetComponent<Card>().RpcSlideToPosition(new Vector3(0,200,-1));
            yield return new WaitForSeconds(3f);

            Assert.AreEqual((new Vector3(0,200,-1)).ToString(), (questionGo.transform.position).ToString());

            questionGo.GetComponent<Card>().RpcSlideToPosition(new Vector3(-100,-200,-1));
            yield return new WaitForSeconds(3f);

            Assert.AreEqual((new Vector3(-100,-200,-1)).ToString(), questionGo.transform.position.ToString());
        }

    }
}