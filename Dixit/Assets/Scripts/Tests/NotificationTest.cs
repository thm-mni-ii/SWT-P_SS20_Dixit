using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Mirror;
using TMPro;

namespace Tests
{
    public class NotificationTest
    {
        private GameObject notificationCanvas;
        private NotificationSystem notificationSystem;
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
            notificationCanvas = GameObject.FindGameObjectWithTag("NotificationCanvas");
            notificationSystem = GameObject.FindGameObjectWithTag("NotificationSystem").GetComponent<NotificationSystem>();

        }

        [UnityTest]
        public IEnumerator SendNotifications()
        {
            yield return new WaitForSeconds(1f);
            
            Utils.GetPlayers().First().TargetSendNotification (
                new Notification ( Notification.NotificationTypes.bad,
                                   "1","testBAD")
            );

            yield return new WaitForSeconds(0.5f);
            var found = false;
            foreach (Transform notification in notificationCanvas.transform)
            {
                var canvas = notification.GetChild(0).gameObject;
                var tmp = canvas.GetComponent<TMP_Text>();

                if(tmp.text == "testBAD")
                {
                    found = true;
                    Assert.AreEqual(notificationSystem.badColor,tmp.color);

                    notification.GetComponent<NotificationCanvas>().showLong();
                    yield return new WaitForSeconds(0.5f);
                    Assert.AreEqual("1", tmp.text);

                    notification.GetComponent<NotificationCanvas>().showShort();
                    yield return new WaitForSeconds(0.5f);
                    Assert.AreEqual("testBAD", tmp.text);

                }
                
            }

            Assert.IsTrue(found);

            Utils.GetPlayers().First().TargetSendNotification (
                new Notification ( Notification.NotificationTypes.good,
                                   "2","testGOOD")
            );

            yield return new WaitForSeconds(0.5f);

            found = false;
            foreach (Transform notification in notificationCanvas.transform)
            {
                var canvas = notification.GetChild(0).gameObject;
                var tmp = canvas.GetComponent<TMP_Text>();

                if(tmp.text == "testGOOD")
                {
                    found = true;
                    Assert.AreEqual(notificationSystem.goodColor,tmp.color);

                    notification.GetComponent<NotificationCanvas>().showLong();
                    yield return new WaitForSeconds(0.5f);
                    Assert.AreEqual("2", tmp.text);

                    notification.GetComponent<NotificationCanvas>().showShort();
                    yield return new WaitForSeconds(0.5f);
                    Assert.AreEqual("testGOOD", tmp.text);

                }
                
            }

            Assert.IsTrue(found);

            Utils.GetPlayers().First().TargetSendNotification (
                new Notification ( Notification.NotificationTypes.regular,
                                   "3","testREG")
            );

            yield return new WaitForSeconds(0.5f);

            found = false;
            foreach (Transform notification in notificationCanvas.transform)
            {
                var canvas = notification.GetChild(0).gameObject;
                var tmp = canvas.GetComponent<TMP_Text>();

                if(tmp.text == "testREG")
                {
                    found = true;
                    Assert.AreEqual(notificationSystem.regularColor,tmp.color);

                    notification.GetComponent<NotificationCanvas>().showLong();
                    yield return new WaitForSeconds(0.5f);
                    Assert.AreEqual("3", tmp.text);

                    notification.GetComponent<NotificationCanvas>().showShort();
                    yield return new WaitForSeconds(0.5f);
                    Assert.AreEqual("testREG", tmp.text);

                }
                
            }

            Assert.IsTrue(found);

            Utils.GetPlayers().First().TargetSendNotification (
                new Notification ( Notification.NotificationTypes.warning,
                                   "4","testWARNING")
            );

            yield return new WaitForSeconds(0.5f);

            found = false;
            foreach (Transform notification in notificationCanvas.transform)
            {
                var canvas = notification.GetChild(0).gameObject;
                var tmp = canvas.GetComponent<TMP_Text>();

                if(tmp.text == "testWARNING")
                {
                    found = true;
                    Assert.AreEqual(notificationSystem.warningColor,tmp.color);

                    notification.GetComponent<NotificationCanvas>().showLong();
                    yield return new WaitForSeconds(0.5f);
                    Assert.AreEqual("4", tmp.text);

                    notification.GetComponent<NotificationCanvas>().showShort();
                    yield return new WaitForSeconds(0.5f);
                    Assert.AreEqual("testWARNING", tmp.text);

                }
                
            }

            Assert.IsTrue(found);

            //test for more the five notifications the oldest is deleted

            Utils.GetPlayers().First().TargetSendNotification (
                new Notification ( Notification.NotificationTypes.warning,
                                   "5","test")
            );

            Utils.GetPlayers().First().TargetSendNotification (
                new Notification ( Notification.NotificationTypes.warning,
                                   "6","test")
            );

            yield return new WaitForSeconds(1f);

            found = false;
            foreach (Transform notification in notificationCanvas.transform)
            {
                if(notification.GetChild(0).gameObject.GetComponent<TMP_Text>().text == "testBAD")
                    found = true;                
            }

            Assert.IsFalse(found);
           
        }
    }
}