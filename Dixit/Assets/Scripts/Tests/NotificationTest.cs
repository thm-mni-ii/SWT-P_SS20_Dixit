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
        public IEnumerator ShowAndColorTest()
        {
            yield return new WaitForSeconds(1f);

            Utils.GetPlayers().First().TargetSendNotification (
                new Notification ( Notification.NotificationTypes.bad,
                                   "1","testBAD")
            );

            Utils.GetPlayers().First().TargetSendNotification (
                new Notification ( Notification.NotificationTypes.good,
                                   "2","testGOOD")
            );

            Utils.GetPlayers().First().TargetSendNotification (
                new Notification ( Notification.NotificationTypes.regular,
                                   "3","testREG")
            );

            Utils.GetPlayers().First().TargetSendNotification (
                new Notification ( Notification.NotificationTypes.warning,
                                   "4","testWARNING")
            );

            yield return new WaitForSeconds(0.5f);
            var found = 0;
            foreach (Transform notification in notificationCanvas.transform)
            {
                var canvas = notification.GetChild(0).gameObject;
                var tmp = canvas.GetComponent<TMP_Text>();

                if(tmp.text == "testBAD")
                {
                    found++;
                    Assert.AreEqual(notificationSystem.badColor,tmp.color);
                }

                if(tmp.text == "testGOOD")
                {
                    found++;
                    Assert.AreEqual(notificationSystem.goodColor,tmp.color);
                }

                if(tmp.text == "testREG")
                {
                    found++;
                    Assert.AreEqual(notificationSystem.regularColor,tmp.color);
                }

                if(tmp.text == "testWARNING")
                {
                    found++;
                    Assert.AreEqual(notificationSystem.warningColor,tmp.color);
                }

            }

            Assert.AreEqual(4, found);
        }


        [UnityTest]
        public IEnumerator ShowLongTest()
        {
            yield return new WaitForSeconds(0.5f);

            Utils.GetPlayers().First().TargetSendNotification (
                new Notification ( Notification.NotificationTypes.regular,
                                   "long explanation","short")
            );

            yield return new WaitForSeconds(0.5f);

            foreach (Transform notification in notificationCanvas.transform)
            {
                var canvas = notification.GetChild(0).gameObject;
                var tmp = canvas.GetComponent<TMP_Text>();

                if(tmp.text == "short")
                {
                    notification.GetComponent<NotificationCanvas>().showLong();
                    yield return new WaitForSeconds(0.5f);
                    Assert.AreEqual("long explanation", tmp.text);

                    notification.GetComponent<NotificationCanvas>().showShort();
                    yield return new WaitForSeconds(0.5f);
                    Assert.AreEqual("short", tmp.text);

                }

            }
        }

        [UnityTest]
        public IEnumerator ShowFiveTest()
        {
             //test for more the five notifications the oldest is deleted
            Utils.GetPlayers().First().TargetSendNotification (
                new Notification ( Notification.NotificationTypes.regular,
                                   "1","testOne")
            );

            Utils.GetPlayers().First().TargetSendNotification (
                new Notification ( Notification.NotificationTypes.regular,
                                   "2","test")
            );
            Utils.GetPlayers().First().TargetSendNotification (
                new Notification ( Notification.NotificationTypes.regular,
                                   "3","test")
            );

            Utils.GetPlayers().First().TargetSendNotification (
                new Notification ( Notification.NotificationTypes.regular,
                                   "4","test")
            );

            Utils.GetPlayers().First().TargetSendNotification (
                new Notification ( Notification.NotificationTypes.regular,
                                   "5","test")
            );

            yield return new WaitForSeconds(1f);

            var found = false;
            foreach (Transform notification in notificationCanvas.transform)
            {
                if(notification.GetChild(0).gameObject.GetComponent<TMP_Text>().text == "testOne")
                    found = true;
            }

            Assert.IsTrue(found);

            Utils.GetPlayers().First().TargetSendNotification (
                new Notification ( Notification.NotificationTypes.regular,
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