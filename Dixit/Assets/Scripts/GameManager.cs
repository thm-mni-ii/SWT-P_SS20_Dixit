/* created by: SWT-P_SS_20_Dixit */
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;
using Firebase.Extensions;


/// <summary>
/// The GameManger keeps track of all given Answers and Changes the curren Phase of the Game.
/// It only exists on the Server.
/// </summary>
public class GameManager : NetworkBehaviour
{
    private readonly Dictionary<NetworkIdentity, string> answers = new Dictionary<NetworkIdentity, string>();
    private readonly Dictionary<NetworkIdentity, NetworkIdentity> choices = new Dictionary<NetworkIdentity, NetworkIdentity>();
    private readonly Dictionary<NetworkIdentity, int> points = new Dictionary<NetworkIdentity, int>();
    private readonly MultivalDictionaty<NetworkIdentity, NetworkIdentity> sameAnswers = new MultivalDictionaty<NetworkIdentity, NetworkIdentity>();

    private NetworkManager NetworkManager => NetworkManager.singleton;

    public GameObject m_cardPrefab;
    public GameObject m_questionCardPrefab;

    private enum Phase { WriteAnswer, ChoseAnswer, Evaluation }
    private Phase currentPhase;

    public CountdownTimer timer;
    public int timerForGiveAnswer = 20;
    public int timerToChooseAnswer = 15;

    public int numberOfRounds = 3;
    private int currentRound;

    //Will be set by Game Host later on
    public string questionSetID = "0";
    public QuestionSet QuestionSet => loadQuestionSet.Result;

    //contains an array with all random, different indexes of the questions for the game
    private int[] indexesOfQuestion;

    private Task<QuestionSet> loadQuestionSet;

    /// <summary>
    /// Called when the GameManger starts on the Server.
    /// </summary>
    public override void OnStartServer()
    {
        // Initializes QuestionSet from given ID
        loadQuestionSet = QuestionSet.RetrieveQuestionSet(questionSetID, GetComponent<DatabaseSetup>().DB).ContinueWithLogException();
    }

    public void StartGame()
    {
        currentRound = 0;
        //wait until the question set is loaded
        loadQuestionSet.ContinueWithOnMainThread(t =>
        {  
            //get random idx array for questions
            indexesOfQuestion = getRandomQuestionIdxArray(QuestionSet.QuestionCount);

            //Start the firt round
            StartRound();
        });       
    }

    private void StartRound()
    {
        currentPhase = Phase.WriteAnswer;

        //get Question for the current round
        QuestionSet.GetQuestion(indexesOfQuestion[currentRound]).ContinueWithLogException().ContinueWithOnMainThread(l =>
        {
            Debug.Log(l.Result.QuestionText);
            answers.Add(this.netIdentity, l.Result.Answer);
            WriteAnswerPhase(l.Result);
        });
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
        timer.StartTimer(timerForGiveAnswer);

        //wait for all players to send answer or get timeout

    }

    private void ChooseAnswerPhase()
    {
        //delete input card at client
        Player.LocalPlayer.RpcDeleteInputCard();

        //Send answers to clients
        SendAnswers();

        // start timer
        timer.StartTimer(timerToChooseAnswer);


    }

    private void EvaluationPhase()
    {
        // eval points
        foreach (var choice in choices){
            if(choice.Key != this.netIdentity && !points.ContainsKey(choice.Key)) points.Add(choice.Key, 0);
            if(choice.Value != this.netIdentity && !points.ContainsKey(choice.Value)) points.Add(choice.Value, 0);

            //player choose right answer -> +3 points
            if (choice.Value == this.netIdentity) points[choice.Key] += 3;
            //player choose own anser -> -1 point
            else if (choice.Key == choice.Value
                || (sameAnswers.ContainsKey(choice.Value) && sameAnswers[choice.Value].Contains(choice.Key))) points[choice.Key] -= 1;
            //player choose answer of other player -> other player get +1 point
            else
            {
                points[choice.Value] += 1;
                if(!sameAnswers.ContainsKey(choice.Value)) return;

                foreach (var player in sameAnswers[choice.Value])
                {
                    points[choice.Value] += 1;
                }
            }
        }

        foreach (var p in points)
        {
            Debug.Log(p.Key.netId + " Points: " + p.Value);
        }
        Player.LocalPlayer.RpcHighlightCard(this.netIdentity);
    }

    /// <summary>
    /// Gets the list of Players in the current Game.
    /// <returns>The list of players.</returns>
    /// </summary>
    private List<Player> GetPlayers() =>
        NetworkServer.connections.Values.Select(c => c.identity.gameObject.GetComponent<Player>()).ToList();

    /// <summary>
    /// Logs the given Answer of a Player during the WriteAnwer Phase.
    /// </summary>
    public void LogAnswer(NetworkIdentity player, string answer)
    {
        foreach(var givenAnswer in answers)
        {
            if(answer == givenAnswer.Value)
            {
                Debug.Log("same Answer!");

                if(givenAnswer.Key != this.netIdentity)
                    sameAnswers.Add(givenAnswer.Key, player);

                return;
            }
        }

        answers.Add(player, answer);
        Debug.Log(answer);

        if (answers.Count == NetworkManager.numPlayers)
        {
            //ChangePhase();
        }
    }

    /// <summary>
    /// Logs the choosen Answer of a Player during the ChoseAnswer Phase.
    /// </summary>
    public void LogAnswer(NetworkIdentity player, NetworkIdentity choice)
    {
        choices.Add(player, choice);
        if (answers.Count == NetworkManager.numPlayers)
        {
            //ChangePhase();
        }
    }

    /// <summary>
    /// Changes Phase in the GameManager and calls the Corresponding RPC-Call
    /// </summary>
    public void ChangePhase()
    {
        switch (currentPhase)
        {
            case Phase.WriteAnswer:
                currentPhase = Phase.ChoseAnswer;
                ChooseAnswerPhase();
                break;
            case Phase.ChoseAnswer:
                currentPhase = Phase.Evaluation;
                EvaluationPhase();
                break;
            case Phase.Evaluation:
                currentPhase = Phase.WriteAnswer;
                break;
            default:
                break;
        }
    }

    private void SendAnswers()
    {

        var startX = (answers.ToArray().Length * 125 + (answers.ToArray().Length - 1) * 20) / 2;

        var xPosition = startX - 62.5;

        foreach (var answer in answers)
        {
            var cardGo = Instantiate(m_cardPrefab, new Vector3((float)xPosition, -100, -2), Quaternion.Euler(0, 0, 0));
            var card = cardGo.GetComponent<Card>();
            card.text = answer.Value;
            card.choosen = answer.Key;
            card.type = Card.CardType.Answer;

            NetworkServer.Spawn(cardGo);
            xPosition -= 145;
        }
    }

    public int[] getRandomQuestionIdxArray(int maxIdx){

        //initialize array for question indexes with -1 for all values
        var randomQuestionIdxArray = new int[numberOfRounds];
        for (int i = 0; i < numberOfRounds; i++) randomQuestionIdxArray[i] = -1;

        for (int i = 0; i < numberOfRounds; i++)
            {
                //get a random value with in not in the array yet and place it in the array for the round 
                var randomQuestionIdx = 0;
                do
                {
                    randomQuestionIdx = UnityEngine.Random.Range(0,maxIdx); 

                } while (Array.IndexOf(randomQuestionIdxArray, randomQuestionIdx)>=0);
                 
                randomQuestionIdxArray[i] = randomQuestionIdx;
            }

        return randomQuestionIdxArray;
    }
}

class MultivalDictionaty<TKey, TValue> : Dictionary<TKey, List<TValue>>
{
    public void Add(TKey key, TValue value)
    {
        if(!this.ContainsKey(key))
            this.Add(key, new List<TValue>());

        this[key].Add(value);
    }
}
