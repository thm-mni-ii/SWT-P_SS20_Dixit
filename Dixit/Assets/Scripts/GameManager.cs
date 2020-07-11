/* created by: SWT-P_SS_20_Dixit */

using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Timers;
using UnityEngine;
using Mirror;
using Firebase.Extensions;
using Random = UnityEngine.Random;


/// <summary>
/// The GameManger keeps track of all given Answers and Changes the curren Phase of the Game.
/// It only exists on the Server.
/// </summary>
public class GameManager : NetworkBehaviour
{
    private readonly Dictionary<UInt32, string> answers = new Dictionary<UInt32, string>();
    private readonly Dictionary<UInt32, UInt32> choices = new Dictionary<UInt32, UInt32>();
    private readonly Dictionary<UInt32, int> points = new Dictionary<UInt32, int>();
    private readonly Dictionary<UInt32, int> roundPoints = new Dictionary<UInt32, int>();

    //for storing points sorted by value
    private List<KeyValuePair<UInt32, int>> pointsList;
    private List<KeyValuePair<UInt32, int>> roundPointsList;
    private readonly MultivalDictionaty<UInt32, UInt32> sameAnswers = new MultivalDictionaty<UInt32, UInt32>();

    private int PlayerCount => GetPlayers().Count;

    public GameObject m_cardPrefab;
    public GameObject m_questionCardPrefab;
    public GameObject m_scoreResultOverlay;

    private enum Phase
    {
        WriteAnswer,
        ChoseAnswer,
        Evaluation
    }

    private Phase currentPhase;

    public CountdownTimer timer;
    public int timerForGiveAnswer = 30;
    public int timerToChooseAnswer = 20;
    public int timerToCheckResults = 10;

    public int numberOfRounds = 3;
    private int currentRound;

    //Will be set by Game Host later on
    public string questionSetID = "0";
    public QuestionSet QuestionSet => loadQuestionSet.Result;

    //contains an array with all random, different indexes of the questions for the game
    private int[] indexesOfQuestion;
    private int playersReady = 0;

    private Task<QuestionSet> loadQuestionSet;

    /// <summary>
    /// Called when the GameManger starts on the Server.
    /// </summary>
    public override void OnStartServer()
    {
        // Initializes QuestionSet from given ID
        loadQuestionSet = QuestionSet.RetrieveQuestionSet(questionSetID, GetComponent<DatabaseSetup>().DB)
            .ContinueWithLogException();
    }

    public void StartGame()
    {
        //initializes PlayerCanvas
        foreach ((Player p, int idx) in GetPlayers().Select(ValueTuple.Create<Player, int>))
        {
            points.Add(p.netIdentity.netId, 0);
            roundPoints.Add(p.netIdentity.netId, 0);
        }

        foreach (Player p1 in GetPlayers())
        {
            p1.TargetUpdateScoreHeader(1);
            foreach ((Player p2, int index) in GetPlayers().Select(ValueTuple.Create<Player, int>))
            {
                p1.TargetUpdatePlayerCanvasEntry(index, p2.PlayerName, "0");
                p1.TargetUpdateTextPanelEntry(index, p2.PlayerName, 0);
            }
        }


        currentRound = -1;
        //wait until the question set is loaded
        loadQuestionSet.ContinueWithOnMainThread(t =>
        {
            //get random idx array for questions
            indexesOfQuestion = GetRandomQuestionIdxArray(QuestionSet.QuestionCount);

            //Start the first round
            currentPhase = Phase.WriteAnswer;
            StartRound();
        });
    }

    private void StartRound()
    {
        foreach (Player p in GetPlayers())
        {
            p.TargetResultOverlaySetActive(false);
        }

        currentRound++;

        if (currentRound >= numberOfRounds)
        {
            EndOfGame();
            return;
        }

        //get Question for the current round
        QuestionSet.GetQuestion(indexesOfQuestion[currentRound]).ContinueWithLogException().ContinueWithOnMainThread(
            l =>
            {
                Debug.Log(l.Result.QuestionText);
                answers.Add(this.netIdentity.netId, l.Result.Answer);
                WriteAnswerPhase(l.Result);
            });
    }

    private void EndOfGame()
    {
        Debug.Log("End Of Game");
        //TODO: show total scores

        //TODO: add scores to framework/ player Info

        //TODO: New Game?

        //else Quit!
    }

    private void WriteAnswerPhase(Question question)
    {
        //render question cards at client
        var cardGo = Instantiate(m_questionCardPrefab, new Vector3(0, 100, -1), Quaternion.identity);
        var card = cardGo.GetComponent<Card>();
        card.text = question.QuestionText;
        card.type = Card.CardType.Question;

        NetworkServer.Spawn(cardGo);


        //render input card at client
        cardGo = Instantiate(m_cardPrefab, new Vector3(0, -100, -1), Quaternion.identity);
        card = cardGo.GetComponent<Card>();
        card.text = question.QuestionText;
        card.type = Card.CardType.Input;

        NetworkServer.Spawn(cardGo);

        // start timer

        StartCoroutine(CheckEarlyTimeout(Phase.WriteAnswer));
        timer.StartTimer(timerForGiveAnswer, CountdownTimer.timerModes.giveAnswer);
        
        //wait for all players to send answer or get timeout
    }

    private IEnumerator CheckEarlyTimeout(Phase forPhase)
    {
        bool done = false;
        while (currentPhase == forPhase && !done)
        {
            if (forPhase == Phase.WriteAnswer && answers.Count >= PlayerCount + 1 ||
                forPhase == Phase.ChoseAnswer && choices.Count >= PlayerCount || forPhase == Phase.Evaluation &&
                NetworkManager.singleton.numPlayers == playersReady)
            {
                timer.StopTimer();
                done = true;
            }
            else
            {
                yield return new WaitForSeconds(1f);
            }
        }
    }

    private void ChooseAnswerPhase()
    {
        // check if any player gave no answer
        foreach (var p in GetPlayers())
        {
            if (!GaveAnswer(p))
            {
                // if player gave no answer, he gets -1 points
                GetPoints(p.netId, -1);
                //send Message to this player
                p.TargetSendNotification("Es muss eine Antwort abgegeben werden.");
                UpdatePlayerCanvas();
            }

            if (AnswerIsEmpty(p.netId)) answers.Remove(p.netId);
        }

        //delete input card at client
        Player.LocalPlayer.RpcDeleteInputCard();

        // if not enough answer are given, to play the round, show the correct answer and go to the next phase
        if (answers.Count < 3)
        {
            string correctAnswer = answers[this.netId];
            answers.Clear();
            answers.Add(this.netId, correctAnswer);

            //Messagesystem Alert not enough answers, resolve round and show correct answer
            foreach (var p in GetPlayers())
            {
                p.TargetSendNotification("Es wurden nicht genug Antworten abgegeben.");
            }

            //Send answer to clients
            SendAnswers();
            StartCoroutine(WaitAndChangePhase());
            return;
        }

        //Send answers to clients
        SendAnswers();

        // start timer
        timer.StartTimer(timerToChooseAnswer, CountdownTimer.timerModes.selectAnswer);
    }

    private void EvaluationPhase()
    {
        // eval points
        foreach (var choice in choices)
        {
            UInt32 playerId = choice.Key;
            UInt32 answerId = choice.Value;

            //player choose own answer -> -1 point
            if (ClickedOnOwnAnswer(playerId, answerId))
            {
                GetPoints(playerId, -1);
            }
            //player choose right answer -> +3 points
            else if (answerId == this.netId)
            {
                GetPoints(playerId, 3);
            }
            else
            {
                //player choose answer of other player -> other player get +1 point
                GetPoints(playerId, 1);
                //all players who gave this anwer get +1 point
                if (sameAnswers.ContainsKey(answerId))
                {
                    foreach (UInt32 p in sameAnswers[answerId])
                        GetPoints(playerId, 1);
                }
            }
        }

        Player.LocalPlayer.RpcHighlightCard(this.netIdentity.netId);

        UpdateScoreResultsOverlay();
        UpdatePlayerCanvas();

        StartCoroutine(WaitAndShowResults());
    }

    private bool ClickedOnOwnAnswer(UInt32 clicker, UInt32 clickedOn) =>
        (clicker == clickedOn) || (sameAnswers.ContainsKey(clickedOn) && sameAnswers[clickedOn].Contains(clicker));

    private bool GaveAnswer(Player p) =>
        (answers.ContainsKey(p.netId) || sameAnswers.Any(pair => pair.Value.Contains(p.netId)));

    private bool AnswerIsEmpty(UInt32 p) =>
        (answers.ContainsKey(p) && answers[p] != "");

    /// <summary>
    /// Updates PlayerCanvas with new scores and ranking in all clients
    /// </summary>
    private void UpdatePlayerCanvas()
    {
        pointsList = points.ToList();
        pointsList.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
        foreach (Player p in GetPlayers())
        {
            int idx = PlayerCount - 1;
            foreach (KeyValuePair<UInt32, int> points in pointsList)
            {
                string player = GetIdentity(points.Key).GetComponent<Player>().PlayerName;
                string playerPoints = points.Value.ToString();
                p.TargetUpdatePlayerCanvasEntry(idx, player, playerPoints);
                idx--;
            }
        }
    }

    private IEnumerator WaitAndShowResults()
    {
        StartCoroutine(CheckEarlyTimeout(Phase.Evaluation));
        timer.StartTimer(timerToCheckResults, CountdownTimer.timerModes.scoreScreen);
        int secs = 3;
        if (answers.Count == 1) secs = 5;
        yield return new WaitForSeconds(secs);
        foreach (Player p in GetPlayers())
        {
            p.TargetResultOverlaySetActive(true);
        }
    }

    private IEnumerator WaitAndChangePhase()
    {
        yield return new WaitForSeconds(1);
        ChangePhase();
    }


    public void LogPlayerIsReady()
    {
        playersReady++;
        if (NetworkManager.singleton.numPlayers == playersReady)
        {
            timer.StopTimer();
        }
    }

    public void InitiateCleanUpEvalPhase()
    {
        playersReady = 0;
        CleanUpEvalPhase();
        ChangePhase();
    }

    private void CleanUpEvalPhase()
    {
        Player.LocalPlayer.RpcDeleteQuestionCard();
        Player.LocalPlayer.RpcDeleteAllAnswerCards();

        answers.Clear();
        choices.Clear();
        sameAnswers.Clear();
        foreach (UInt32 p in roundPoints.Keys.ToArray())
            roundPoints[p] = 0;
    }

    /// <summary>
    /// Updates ScoreResultsOverlay with new scores and ranking in all clients
    /// </summary>
    private void UpdateScoreResultsOverlay()
    {
        roundPointsList = roundPoints.ToList();
        roundPointsList.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
        foreach (Player p in GetPlayers())
        {
            p.TargetUpdateScoreHeader(currentRound + 1);
            int idx = PlayerCount - 1;
            foreach (KeyValuePair<UInt32, int> roundPoints in roundPointsList)
            {
                string player = GetIdentity(roundPoints.Key).GetComponent<Player>().PlayerName;
                int playerPoints = roundPoints.Value;
                p.TargetUpdateTextPanelEntry(idx, player, playerPoints);
                idx--;
            }
        }
    }

    private void GetPoints(UInt32 player, int newPoints)
    {
        roundPoints[player] += newPoints;
        points[player] += newPoints;
    }

    /// <summary>
    /// Gets the list of Players in the current Game.
    /// <returns>The list of players.</returns>
    /// </summary>
    private List<Player> GetPlayers() =>
        NetworkServer.connections.Values.Select(c => c.identity.gameObject.GetComponent<Player>()).ToList();

    /// <summary>
    /// Gets the NetworkIdentity component of an object with the specified netId
    /// <param name="netId"> The Identity to be searched for </param>
    /// <returns> The NetworkIdentity </returns>
    /// </summary>
    private NetworkIdentity GetIdentity(UInt32 netId) =>
        NetworkServer.connections.Values.Where(c => c.identity.netId == netId).Select(c => c.identity).First();

    /// <summary>
    /// Logs the given Answer of a Player during the WriteAnwer Phase.
    /// </summary>
    public void LogAnswer(UInt32 playerId, string answer)
    {
        var player = GetIdentity(playerId).GetComponent<Player>();
        if (currentPhase != Phase.WriteAnswer)
        {
            Debug.LogWarning($"LogAnswer (WriteAnswer Phase) called during {currentPhase} by player {playerId}!");
            return;
        }

        // check if any player gave no answer
        if (answer == "")
        {
            // if player gave no answer, he gets -1 points
            GetPoints(playerId, -1);
            //send Message to this player
            player.TargetSendNotification("Es muss eine Antwort abgegeben werden.");
            UpdatePlayerCanvas();
        }

        //filter out duplicate answers
        foreach (var givenAnswer in answers)
        {
            if (answer.ToLower() == givenAnswer.Value.ToLower())
            {
                // if player gave correct answer, the player get -1 points
                if (givenAnswer.Key == this.netId)
                {
                    GetPoints(playerId, -1);
                    //send notification
                    player.TargetSendNotification("Es muss eine falsche Antwort abgegeben werden.");
                    UpdatePlayerCanvas();
                }

                sameAnswers.Add(givenAnswer.Key, playerId);

                return;
            }
        }

        answers.Add(playerId, answer);
    }

    /// <summary>
    /// Logs the choosen Answer of a Player during the ChoseAnswer Phase.
    /// </summary>
    public void LogAnswer(UInt32 player, UInt32 choice)
    {
        if (currentPhase != Phase.ChoseAnswer)
        {
            Debug.LogWarning($"LogAnswer (ChoseAnswer Phase) called during {currentPhase} by player {player}!");
            return;
        }

        if (choices.ContainsKey(player))
            choices[player] = choice;
        else
            choices.Add(player, choice);
    }

    /// <summary>
    /// Changes Phase in the GameManager and calls the Corresponding RPC-Call
    /// </summary>
    public void ChangePhase()
    {
        currentPhase = (Phase) ((int) (currentPhase + 1) % Enum.GetValues(typeof(Phase)).Length);

        switch (currentPhase)
        {
            case Phase.WriteAnswer:
                StartRound();
                break;
            case Phase.ChoseAnswer:
                ChooseAnswerPhase();
                break;
            case Phase.Evaluation:
                EvaluationPhase();
                break;
            default:
                Debug.LogError("Unknown Phase: " + currentPhase);
                break;
        }
    }

    private void SendAnswers()
    {
        double startX = ((answers.Count * 125) + ((answers.Count - 1) * 20)) / 2;

        double xPosition = startX - 62.5;
        int index = 0;

        var answersArray = answers.ToArray();

        ShuffleArray(answersArray);

        foreach (var answer in answersArray)
        {
            var cardGo = Instantiate(m_cardPrefab, new Vector3(0, -100, -(2 + index)), Quaternion.Euler(0, 0, 0));
            var card = cardGo.GetComponent<Card>();

            card.text = answer.Value;
            card.id = answer.Key;
            card.type = Card.CardType.Answer;
            card.startFacedown = true;

            NetworkServer.Spawn(cardGo);
            card.RpcSlideToPosition(new Vector3((float) xPosition, -100, -2));
            card.RpcFlip(false, false, (float) ((index * 0.2) + 1));

            xPosition -= 145;

            index++;
        }
    }

    private void ShuffleArray<T>(T[] array)
    {
        // Knuth shuffle algorithm :: courtesy of Wikipedia :)
        for (int t = 0; t < array.Length; t++)
        {
            T tmp = array[t];
            int r = Random.Range(t, array.Length);
            array[t] = array[r];
            array[r] = tmp;
        }
    }

    public int[] GetRandomQuestionIdxArray(int maxIdx)
    {
        var randomQuestionIdxList = new List<int>(numberOfRounds);

        for (int i = 0; i < numberOfRounds; i++)
        {
            //get a random value which is not in the array yet and place it in the array for the round 
            int randomQuestionIdx;
            do
            {
                randomQuestionIdx = Random.Range(0, maxIdx);
            } while (randomQuestionIdxList.Contains(randomQuestionIdx));

            randomQuestionIdxList.Add(randomQuestionIdx);
        }

        return randomQuestionIdxList.ToArray();
    }
}

class MultivalDictionaty<TKey, TValue> : Dictionary<TKey, List<TValue>>
{
    public void Add(TKey key, TValue value)
    {
        if (!this.ContainsKey(key))
            this.Add(key, new List<TValue>());

        this[key].Add(value);
    }
}