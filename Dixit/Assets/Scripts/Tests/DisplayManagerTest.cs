/* created by: SWT-P_SS_20_Dixit */

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using TMPro;
using Mirror;

namespace Tests
{
    public class DisplayManagerTest
    {
        DisplayManager displayManager;
        GameManager gameManager;
        bool serverStarted = false;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            if (!serverStarted)
            {
                GameObject gso = MonoBehaviour.Instantiate(Resources.Load("Prefabs/NetworkManager")) as GameObject;
                var gameServer = gso.GetComponent<GameServer>();
                gameServer.StartHost();
                serverStarted = true;
                yield return new WaitForSeconds(1f);
                var conn = new NetworkConnectionToClient(2);
                NetworkServer.AddConnection(conn);
                gameServer.OnServerAddPlayer(conn);

                yield return new WaitForSeconds(1f);

                foreach ((var player, var idx) in Utils.GetPlayersIndexed())
                {
                    if (player.PlayerName == null || player.PlayerName == "")
                    {
                        player.PlayerName = "Mustermann" + idx;
                        player.CmdSendName("Mustermann" + idx);
                    }
                }

                yield return new WaitForSeconds(1f);
                displayManager = GameObject.FindGameObjectWithTag("DisplayManager").GetComponent<DisplayManager>();
                gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
                gameManager.TimerForGiveAnswer = 5;
                gameManager.TimerToCheckResults = 3;
                gameManager.TimerToChooseAnswer = 3;
            }
        }

        [UnityTest]
        public IEnumerator DeletedCards()
        {
            yield return DeletedInputCard();
            yield return DeletedAnswerCards();
            yield return DeletedQuestionCard();
        }

        private IEnumerator DeletedInputCard()
        {
            foreach ((Player p, int i) in Utils.GetPlayersIndexed())
            {
                gameManager.LogAnswer(p.netId, $"Test {i}");
            }
            yield return new WaitForSeconds(2f);
            Assert.IsNull(GameObject.FindGameObjectWithTag("InputCard"), "Input card not deleted");
            yield return null;
        }

        private IEnumerator DeletedAnswerCards()
        {
            displayManager.RpcDeleteAllAnswerCards();
            yield return new WaitForFixedUpdate();
            Assert.IsEmpty(GameObject.FindGameObjectsWithTag("AnswerCard"), "Answer cards not deleted");
        }

        private IEnumerator DeletedQuestionCard()
        {
            displayManager.RpcDeleteQuestionCard();
            yield return new WaitForFixedUpdate();
            Assert.IsNull(GameObject.FindGameObjectWithTag("QuestionCard"), "Question card not deleted");
        }

        [UnityTest]
        public IEnumerator CorrectScoreResultOverlay()
        {
            displayManager.RpcResultOverlaySetActive(true);
            yield return new WaitForSeconds(0.3f);
            var sro = GameObject.FindGameObjectWithTag("ScoreResultOverlay");
            Assert.IsTrue(sro.activeInHierarchy, "Score result overlay not active");
            Assert.IsTrue(displayManager.continueButton.GetComponent<Button>().interactable, "Continue button not interactible");
            Assert.IsFalse(displayManager.exitButton.activeInHierarchy, "Exit Button is visible");

            yield return UpdateScoreHeader();
            yield return UpdateScoreEntry();
            yield return ExplanationCorrect();
            yield return Endscreen();

            displayManager.RpcResultOverlaySetActive(false);
            yield return new WaitForSeconds(0.3f);
            Assert.IsFalse(sro.activeInHierarchy, "Score result overlay still active");
        }

        private IEnumerator UpdateScoreHeader()
        {
            displayManager.UpdateScoreHeader(2);
            yield return new WaitForEndOfFrame();
            Assert.AreEqual("~ Punkte in Runde 2 ~", displayManager.ScoreHeader.text, "Score header not correctly formatted (round number)");
            displayManager.RpcUpdateScoreHeaderText("Test");
            yield return new WaitForEndOfFrame();
            Assert.AreEqual("Test", displayManager.ScoreHeader.text, "Score header not correctly formatted (Text)");
        }

        private IEnumerator UpdateScoreEntry()
        {
            displayManager.UpdateTextPanelEntry(0, "tst", 3, false);
            yield return new WaitForEndOfFrame();
            var entry = displayManager.TextPanelEntry[0].GetComponentsInChildren<TextMeshProUGUI>();
            Assert.AreEqual("tst", entry[0].text, "Player name not equal");
            Assert.AreEqual("+3", entry[1].text, "Not the correct points number or format (not end of game)");
            displayManager.UpdateTextPanelEntry(0, "tst", 2, false);
            yield return new WaitForEndOfFrame();
            Assert.AreEqual("+2", entry[1].text, "Points not correctly updated (not end of game");
            displayManager.UpdateTextPanelEntry(0, "tst", 5, true);
            yield return new WaitForEndOfFrame();
            Assert.AreEqual("5", entry[1].text, "Point not correctly formated (end of game)");
        }

        private IEnumerator ExplanationCorrect()
        {
            displayManager.RpcUpdateExplanation("Test Test");
            displayManager.RpcToggleExplanation(true);
            yield return new WaitForEndOfFrame();
            Assert.IsFalse(displayManager.explanationButton.activeInHierarchy, "Explain button is active in explanaition");
            Assert.IsTrue(displayManager.scoreButton.activeInHierarchy, "Score button is not active in explanatioin");
            Assert.IsFalse(displayManager.TextPanel.activeInHierarchy, "Score display is active in explanation");
            Assert.IsTrue(displayManager.Explanations.activeInHierarchy, "Explanation is not active in explanation");
            Assert.AreEqual("Test Test", GameObject.Find("Explanation").GetComponent<TextMeshProUGUI>().text, "Explanation test not updated");
            displayManager.RpcToggleExplanation(false);
            yield return new WaitForEndOfFrame();
            Assert.IsTrue(displayManager.explanationButton.activeInHierarchy, "Explain button is not active in score overview");
            Assert.IsFalse(displayManager.scoreButton.activeInHierarchy, "Score button is active in score overview");
            Assert.IsTrue(displayManager.TextPanel.activeInHierarchy, "Score display is not active in score overview");
            Assert.IsFalse(displayManager.Explanations.activeInHierarchy, "Explanation is active in score overview");
        }

        private IEnumerator Endscreen()
        {
            displayManager.RpcToggleExit(true);
            yield return new WaitForEndOfFrame();
            Assert.IsFalse(displayManager.continueButton.activeInHierarchy, "Score Button is visible on endscreen");
            Assert.IsFalse(displayManager.explanationButton.activeInHierarchy, "Explanation button is visible on endscreen");
            Assert.IsFalse(displayManager.scoreButton.activeInHierarchy, "Score button is visible on endscreen");
            Assert.IsTrue(displayManager.exitButton.activeInHierarchy, "Exit button is not visible on endscreen");
        }

        [UnityTest]
        public IEnumerator PlayerCanvasTest()
        {
            displayManager.RpcUpdatePlayerCanvasEntry(0, "tst", "3");
            yield return new WaitForEndOfFrame();
            var entry = displayManager.PlayerCanvasEntry[0].GetComponentsInChildren<TextMeshProUGUI>();
            Assert.AreEqual("tst", entry[0].text, "Player name not equal");
            Assert.AreEqual("3", entry[1].text, "Not the correct points format");
        }
    }
}
