/* created by: SWT-P_SS_20_Dixit */

using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using Mirror;
using Firebase.Extensions;
using Random = UnityEngine.Random;


/// <summary>
/// The GameManger keeps track of all given Answers and Changes the current Phase of the Game.
/// It only exists on the Server.
/// </summary>
/// \author SWT-P_SS_20_Dixit
public class GameManager : NetworkBehaviour
{
    private readonly Dictionary<UInt32, string> answers = new Dictionary<UInt32, string>();
    private readonly Dictionary<UInt32, UInt32> choices = new Dictionary<UInt32, UInt32>();
    public Dictionary<UInt32, int> points { get; } = new Dictionary<UInt32, int>();
    private Dictionary<UInt32, int>[] roundPoints;

    //for storing points sorted by value
    private List<KeyValuePair<UInt32, int>> pointsList;
    private readonly MultivalDictionary<UInt32, UInt32> sameAnswers = new MultivalDictionary<UInt32, UInt32>();

    private int PlayerCount => Utils.GetPlayers().Count();


    /// <summary>
    /// The singleton instance of the GameManager
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public static GameManager Instance => _instance.Value;
    private static readonly Lazy<GameManager> _instance =
        new Lazy<GameManager>(() => GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>());


    /// <summary>
    /// The prefab for answer cards
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public GameObject m_cardPrefab;

    /// <summary>
    /// The prefab for answer cards
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public GameObject m_questionCardPrefab;

    /// <summary>
    /// The DisplayManager instance in the scene
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public DisplayManager displayManager;

    /// <summary>
    /// PlayerCanvasNames Textgroup to be resized similarly
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public TextResizer PlayerCanvasNames;
    /// <summary>
    /// PlayerCanvasScores Textgroup to be resized similarly
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public TextResizer PlayerCanvasScores;
    /// <summary>
    /// ScoreResultsNames Textgroup to be resized similarly
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public TextResizer ScoreResultsNames;
    /// <summary>
    /// ScoreResultsScores Textgroup to be resized similarly
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public TextResizer ScoreResultsScores;

    private enum Phase
    {
        WriteAnswer,
        ChoseAnswer,
        Evaluation
    }

    private Phase currentPhase;

    /// <summary>
    /// The Timer seen on the right of the Screen
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public CountdownTimer timer;
    /// <summary>
    /// Initial value of the Timer at the start of the "GiveAnswer" Phase
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public int timerForGiveAnswer { get; set;} = 30;
    /// <summary>
    /// Initial value of the Time at the start of the "ChoseAnswer" Phase
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public int timerToChooseAnswer { get; set;} = 20;
    /// <summary>
    /// Initial value of the Timer when the score result overlay is diplayed
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public int timerToCheckResults { get; set;} = 10;

    /// <summary>
    /// The number rounds (i.e Questions) the game should last
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public int numberOfRounds = 3;
    private int currentRound;

    /// <summary>
    /// The ID of the question set played with. Will be set by Game Host coosing the set.
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public string questionSetID = "0";

    /// <summary>
    /// The Question Set load form the database.
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public QuestionSet QuestionSet => loadQuestionSet.Result;

    //contains an array with all random, different indexes of the questions for the game
    private int[] indexesOfQuestion;
    private int playersReady = 0;

    private Task<QuestionSet> loadQuestionSet;

    /// <summary>
    /// Called when the GameManger starts on the Server.
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public override void OnStartServer()
    {
        // Initializes QuestionSet from given ID
        loadQuestionSet = QuestionSet.RetrieveQuestionSet(questionSetID, GetComponent<DatabaseSetup>().DB)
            .ContinueWithLogException();
    }


    /// <summary>
    /// Executed when the game should start (after all players joined).
    /// Writes the playernames in the UI cavaces and loads the chosen question set
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public void StartGame()
    {
        //initializes roundpoints
        roundPoints = new Dictionary<UInt32, int>[numberOfRounds];
        for (int i = 0; i < numberOfRounds; i++)
        {
            roundPoints[i] = new Dictionary<UInt32, int>();
        }

        //initializes points and roundpoints
        foreach ((Player p, int idx) in Utils.GetPlayersIndexed())
        {
            points.Add(p.netIdentity.netId, 0);

            for (int i = 0; i < numberOfRounds; i++)
            {
                roundPoints[i].Add(p.netIdentity.netId, 0);
            }
        }

        UpdatePlayerCanvas();

        currentRound = -1;
        //wait until the question set is loaded
        loadQuestionSet.ContinueWithOnMainThread(t =>
        {
            //get random idx array for questions
            indexesOfQuestion = Utils.GetRandomQuestionIdxArray(QuestionSet.QuestionCount, numberOfRounds);

            //Start the first round
            currentPhase = Phase.WriteAnswer;
            StartRound();
        });
    }

    /// <summary>
    /// Executed at the start of each round.
    /// Hides the result overlay, checks if the game should end and adds the correct answer to <c>answers<\c>
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    private void StartRound()
    {
        displayManager.RpcResultOverlaySetActive(false);
        displayManager.RpcToggleExplanation(false);

        currentRound++;

        if (currentRound >= numberOfRounds)
        {
            EndGame();
            return;
        }

        //get Question for the current round
        QuestionSet.GetQuestion(indexesOfQuestion[currentRound]).ContinueWithLogException().ContinueWithOnMainThread(
            l =>
            {
                Debug.Log(l.Result.QuestionText);
                answers.Add(this.netIdentity.netId, l.Result.Answer);
                displayManager.RpcUpdateExplanation(l.Result.Explanation);
                WriteAnswerPhase(l.Result);
            });
    }

    /// <summary>
    /// Called by <c>StartRound</c> when all rounds have been played.
    /// Shows an overview of the points awarded in each round and the final scores.
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    private void EndGame()
    {
        Debug.Log("End Of Game");

        // show total scores
        UpdateScoreResultsOverlay(true);
        displayManager.RpcUpdateScoreHeaderText("~ Gesamtübersicht ~");

        displayManager.RpcToggleRoundsOverview(true,numberOfRounds);

        displayManager.RpcToggleExit(true);

        displayManager.RpcResultOverlaySetActive(true);

        //set placements of Players
        int idx = 1;
        foreach (KeyValuePair<UInt32, int> points in pointsList)
        {
            Player player = Utils.GetIdentity(points.Key).GetComponent<Player>();
            player.Placement = idx;
            idx++;
        }
    }

    /// <summary>
    /// Returns the name of the winner.
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public string GetNameOfWinner() => Utils.GetIdentity(pointsList[0].Key).GetComponent<Player>().PlayerName; 
    

    private void WriteAnswerPhase(Question question)
    {
        displayManager.RpcShowNormalTimer();
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
        
        if (currentRound == 0)
        {
            foreach (var p in Utils.GetPlayers())
            {
                p.TargetSendTutorialNotification(new Notification(Notification.NotificationTypes.regular,"Geb' eine falsche Antwort, die aber trotzdem plausibel klingt.","1. Antwort Geben"));
            }
        }
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
    
    private IEnumerator CheckScoreTimerStart()
    {
        while (playersReady < PlayerCount/2)
        { 
            yield return new WaitForSeconds(1f);
        }
        displayManager.RpcShowOverlayTimer();
        StartCoroutine(CheckEarlyTimeout(Phase.Evaluation));
        timer.StartTimer(timerToCheckResults, CountdownTimer.timerModes.scoreScreen);
        
    }

    private void ChooseAnswerPhase()
    {
        displayManager.RpcShowNormalTimer();
        if (currentRound == 0)
        {
            foreach (var p in Utils.GetPlayers())
            {
                p.TargetSendTutorialNotification(new Notification(Notification.NotificationTypes.regular,"Wähle die Antwort aus, von der du glaubst, dass es die Richtige ist.","2. Antwort Wählen"));
            }
        }
        // check if any player gave no answer
        foreach (var p in Utils.GetPlayers())
        {
            if (!Utils.GaveAnswer(p, answers, sameAnswers))
            {
                // if player gave no answer, he gets -1 points
                GetPoints(p.netId, -1);
                //send Message to this player
                p.TargetSendNotification(new Notification(Notification.NotificationTypes.bad,"Es muss eine Antwort abgegeben werden!","-1 Punkt"));
                UpdatePlayerCanvas();
            }

            if (Utils.AnswerIsEmpty(p.netId, answers)) answers.Remove(p.netId);
        }

        //delete input card at client
        displayManager.RpcDeleteInputCard();

        // if not enough answer are given, to play the round, show the correct answer and go to the next phase
        if (answers.Count < 3)
        {
            string correctAnswer = answers[this.netId];
            answers.Clear();
            answers.Add(this.netId, correctAnswer);

            //Messagesystem Alert not enough answers, resolve round and show correct answer
            foreach (var p in Utils.GetPlayers())
            {
                p.TargetSendNotification(new Notification(Notification.NotificationTypes.warning,"Es wurden nicht genügend Antworten abgegeben.","+0 Punkte"));
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
        displayManager.RpcHideAllTimers();
        Dictionary<UInt32, int> numOfSuccessfulDeceptionsPerPlayer = new Dictionary<uint, int>();
        // eval points
        foreach (var choice in choices)
        {
            UInt32 playerId = choice.Key;
            UInt32 answerId = choice.Value;

            //player choose own answer -> -1 point
            if (Utils.ClickedOnOwnAnswer(playerId, answerId, sameAnswers))
            {
                GetPoints(playerId, -1);
                Utils.GetIdentity(playerId).gameObject.GetComponent<Player>().TargetSendNotification(new Notification(Notification.NotificationTypes.bad,"Du hast deine eigene Antwort ausgewählt...","-"+1+" Punkte"));
            }
            //player choose right answer -> +3 points
            else if (answerId == this.netId)
            {
                GetPoints(playerId, 3);
                Utils.GetIdentity(playerId).gameObject.GetComponent<Player>().TargetSendNotification(new Notification(Notification.NotificationTypes.good,"Gut! Du hast die richtige Antwort gewählt!","+"+3+" Punkte"));
            }
            else
            {
                //player choose answer of other player -> other player get +1 point
                GetPoints(answerId, 1);
                if (numOfSuccessfulDeceptionsPerPlayer.ContainsKey(answerId))
                {
                    numOfSuccessfulDeceptionsPerPlayer[answerId]++;
                }
                else
                {
                    numOfSuccessfulDeceptionsPerPlayer.Add(answerId,0);
                    numOfSuccessfulDeceptionsPerPlayer[answerId]++;
                }

                //all players who gave this anwer get +1 point
                if (sameAnswers.ContainsKey(answerId))
                {
                    foreach (UInt32 p in sameAnswers[answerId])
                        GetPoints(p, 1);
                }
            }
        }
        
        foreach (var p in numOfSuccessfulDeceptionsPerPlayer)
        {
            if (p.Value == 0)
            {
                Utils.GetIdentity(p.Key).gameObject.GetComponent<Player>().TargetSendNotification(new Notification(Notification.NotificationTypes.regular,"Niemand hat auf deine Antwort geklickt.","+"+0+" Punkte"));
            }
            else
            {
                Utils.GetIdentity(p.Key).gameObject.GetComponent<Player>().TargetSendNotification(new Notification(Notification.NotificationTypes.good,"Sehr gut, du hast "+p.Value+" deiner Mitspieler getäuscht","+"+p.Value+" Punkte"));
            }

        }

        UpdateCards();

        UpdateScoreResultsOverlay(false);
        UpdatePlayerCanvas();

        StartCoroutine(WaitAndShowResults());
    }
    
    private void UpdateCards()
    {
        var cards = GameObject.FindGameObjectsWithTag("AnswerCard").Select(go => go.GetComponent<Card>());
        foreach (var card in cards)
        {
            displayManager.RpcUpdateCard(this.netId, card.gameObject, choices.Values.Where(c => c == card.id).Count());
        }
    }

    

    private IEnumerator WaitAndShowResults()
    {
        StartCoroutine(CheckScoreTimerStart());
        int secs = 3;
        if (answers.Count == 1) secs = 5;
        yield return new WaitForSeconds(secs);
        displayManager.RpcResultOverlaySetActive(true);
        
            
    }

    private IEnumerator WaitAndChangePhase()
    {
        yield return new WaitForSeconds(1);
        ChangePhase();
    }


    /// <summary>
    /// Counts the players who clicked on the "Weiter" Button. When all players clicked on the button, it stops the timer
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public void LogPlayerIsReady()
    {
        playersReady++;
        if (NetworkManager.singleton.numPlayers == playersReady)
        {
            timer.StopTimer();
        }
    }

    /// <summary>
    /// Clean up after the end of the eval phase
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public void InitiateCleanUpEvalPhase()
    {
        playersReady = 0;
        CleanUpEvalPhase();
        ChangePhase();
    }

    private void CleanUpEvalPhase()
    {
        displayManager.RpcDeleteQuestionCard();
        displayManager.RpcDeleteAllAnswerCards();

        answers.Clear();
        choices.Clear();
        sameAnswers.Clear();
    }

    /// <summary>
    /// Updates PlayerCanvas with new scores and ranking in all clients
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    private void UpdatePlayerCanvas()
    {
        pointsList = points.ToList();
        pointsList.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));

        int idx = PlayerCount - 1;
        foreach (KeyValuePair<UInt32, int> points in pointsList)
        {
            string player = Utils.GetIdentity(points.Key).GetComponent<Player>().PlayerName;
            string playerPoints = points.Value.ToString();
            displayManager.RpcUpdatePlayerCanvasEntry(idx, player, playerPoints);
            idx--;
        }
        PlayerCanvasNames.RpcAssimilateSize();
        PlayerCanvasScores.RpcAssimilateSize();
    }

    /// <summary>
    /// Updates ScoreResultsOverlay with new scores and ranking in all clients
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    private void UpdateScoreResultsOverlay(bool gameend)
    {
        var list = gameend? points.ToList() : roundPoints[currentRound].ToList();
        list.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));
        displayManager.UpdateScoreHeader(currentRound + 1);

        if(gameend) pointsList = list;

        int idx = 0;
        foreach (KeyValuePair<UInt32, int> points in list)
        {
            string player = Utils.GetIdentity(points.Key).GetComponent<Player>().PlayerName;
            int playerPoints = points.Value;
            displayManager.UpdateTextPanelEntry(idx, player, playerPoints, gameend);

            if(gameend)
            {
                var p_roundpoints = new int[numberOfRounds];
                for (int i = 0; i < numberOfRounds; i++)
                    p_roundpoints[i] = roundPoints[i][points.Key];

                displayManager.RpcSetRoundOverview(idx == 0, numberOfRounds, p_roundpoints);
            }

            idx++;
        }
        ScoreResultsNames.RpcAssimilateSize();
        ScoreResultsScores.RpcAssimilateSize();
    }

    private void GetPoints(UInt32 player, int newPoints)
    {
        roundPoints[currentRound][player] += newPoints;
        points[player] += newPoints;
    }

    /// <summary>
    /// Logs the given Answer of a Player during the WriteAnwer Phase.
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public void LogAnswer(UInt32 playerId, string answer)
    {
        var player = Utils.GetIdentity(playerId).GetComponent<Player>();
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
            player.TargetSendNotification(new Notification(Notification.NotificationTypes.bad,"Es muss eine Antwort abgegeben werden!","-1 Punkt"));
            UpdatePlayerCanvas();
        }

        //filter out duplicate answers
        foreach (var givenAnswer in answers)
        {
            if (Utils.AnswersAreEqual(answer, givenAnswer.Value))
            {
                // if player gave correct answer, the player get -1 points
                if (givenAnswer.Key == this.netId)
                {
                    GetPoints(playerId, -1);
                    //send notification
                    player.TargetSendNotification(new Notification(Notification.NotificationTypes.bad,"Es muss eine falsche Antwort abgegeben werden!","-1 Punkt"));
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
    /// \author SWT-P_SS_20_Dixit
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
    /// \author SWT-P_SS_20_Dixit
    public void ChangePhase()
    {
        currentPhase = (Phase)((int)(currentPhase + 1) % Enum.GetValues(typeof(Phase)).Length);

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

        Utils.ShuffleArray(answersArray);

        foreach (var answer in answersArray)
        {
            var cardGo = Instantiate(m_cardPrefab, new Vector3(0, -100, -(2 + index)), Quaternion.Euler(0, 0, 0));
            var card = cardGo.GetComponent<Card>();

            card.text = answer.Value;
            card.id = answer.Key;
            card.type = Card.CardType.Answer;
            card.startFacedown = true;
            card.playerName = card.id == this.netId ? "" : Utils.GetIdentity(card.id).GetComponent<Player>().PlayerName;

            NetworkServer.Spawn(cardGo);
            card.RpcSlideToPosition(new Vector3((float)xPosition, -100, -2));
            card.RpcFlip(false, false, (float)((index * 0.2) + 1));

            xPosition -= 145;

            index++;
        }
    }
}
