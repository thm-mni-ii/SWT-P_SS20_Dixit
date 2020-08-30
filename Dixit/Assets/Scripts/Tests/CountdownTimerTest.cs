/* created by: SWT-P_SS_20_Dixit */

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TMPro;

namespace Tests
{
    public class CountdownTimerTest
    {

        public GameServer gameServer;
        public GameManager gameManager;
        private bool serverStarted = false;

        [UnitySetUp]
        public IEnumerator Setup()
        {
            if (!serverStarted)
            {
                var gso = MonoBehaviour.Instantiate(Resources.Load("Prefabs/NetworkManager")) as GameObject;
                gameServer = gso.GetComponent<GameServer>();
                gameServer.StartHost();
                serverStarted = true;
                yield return new WaitForSeconds(1f);

                gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

            }
        }

        [UnityTest]
        public IEnumerator TimerTest()
        {
            var timer = GameObject.Find("UICanvas").transform.GetChild(2).GetChild(0).gameObject.GetComponent<TMP_Text>();

            gameManager.timer.StartTimer(5, CountdownTimer.TimerModes.defaultMode);
            yield return new WaitForSeconds(0.5f);
            Assert.AreEqual("5s", timer.text);

            yield return new WaitForSeconds(2f);
            Assert.AreEqual("3s", timer.text);

            yield return new WaitForSeconds(2f);
            Assert.AreEqual("1s", timer.text);

            yield return new WaitForSeconds(0.5f);
            Assert.AreEqual("0s", timer.text);
        }
    }
}