﻿/* created by: SWT-P_SS_20_Dixit */
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
/// The GameManger keeps track of all given Answers and Changes the curren Phase of the Game.
/// It only exists on the Server.
/// </summary>
public class GameManager : NetworkBehaviour
{
    private readonly Dictionary<NetworkIdentity, string> answers = new Dictionary<NetworkIdentity, string>();
    private readonly Dictionary<NetworkIdentity, NetworkIdentity> choices = new Dictionary<NetworkIdentity, NetworkIdentity>();
    private Dictionary<NetworkIdentity, int> points { get; set; }
    private Dictionary<NetworkIdentity, int> roundPoints { get; set; }

    //for storing points sorted by value
    private List<KeyValuePair<NetworkIdentity, int>> pointsList;
    private List<KeyValuePair<NetworkIdentity, int>> roundPointsList;
    private readonly MultivalDictionaty<NetworkIdentity, NetworkIdentity> sameAnswers = new MultivalDictionaty<NetworkIdentity, NetworkIdentity>();

    private NetworkManager NetworkManager => NetworkManager.singleton;

    private int PlayerCount => GetPlayers().Count; 

    public GameObject m_cardPrefab;
    public GameObject m_questionCardPrefab;
    public GameObject m_scoreResultOverlay;

    private enum Phase { WriteAnswer, ChoseAnswer, Evaluation }
    private Phase currentPhase;

    public CountdownTimer timer;
    public int timerForGiveAnswer = 30;
    public int timerToChooseAnswer = 20;

    public int numberOfRounds = 3;
    private int currentRound;

    //Will be set by Game Host later on
    public string questionSetID = "0";
    public QuestionSet QuestionSet => loadQuestionSet.Result;

    //contains an array with all random, different indexes of the questions for the game
    private int[] indexesOfQuestion;
    private int playersReady=0;

    private Task<QuestionSet> loadQuestionSet;

    /// <summary>
    /// Called when the GameManger starts on the Server.
    /// </summary>
    public override void OnStartServer()
    {
        points = new Dictionary<NetworkIdentity, int>();
        // Initializes QuestionSet from given ID
        loadQuestionSet = QuestionSet.RetrieveQuestionSet(questionSetID, GetComponent<DatabaseSetup>().DB).ContinueWithLogException();
    }

    public void StartGame()
    {
        //Sets dummy playernames and initializes PlayerCanvas (TODO: setting and getting actual names)
        int idx=1;
        foreach(Player p in GetPlayers()){
            p.playerName = "Player"+idx;
            idx++;
        }
        foreach(Player p1 in GetPlayers()){
            int index=0;
            foreach(Player p2 in GetPlayers()){
                p1.TargetUpdatePlayerCanvasEntry(index,p2.playerName, "0");
                index++;
            }
        }
        
        foreach(Player p1 in GetPlayers()){
            int index=0;
            p1.TargetUpdateScoreHeader(1);
            foreach(Player p2 in GetPlayers()){
                p1.TargetUpdateTextPanelEntry(index,p2.playerName, 0);
                index++;
            }
        }

        currentRound = -1;
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
        foreach(Player p in GetPlayers()){
            p.TargetResultOverlaySetActive(false);
        }
        
        currentRound++;

        if (currentRound >= numberOfRounds){
            EndOfGame();
            return;
        }
        
        currentPhase = Phase.WriteAnswer;

        //get Question for the current round
        QuestionSet.GetQuestion(indexesOfQuestion[currentRound]).ContinueWithLogException().ContinueWithOnMainThread(l =>
        {
            Debug.Log(l.Result.QuestionText);
            answers.Add(this.netIdentity, l.Result.Answer);
            WriteAnswerPhase(l.Result);
        });
    }

    private void EndOfGame(){

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
        roundPoints = new Dictionary<NetworkIdentity, int>();
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

            if(choice.Key != this.netIdentity && !roundPoints.ContainsKey(choice.Key)) roundPoints.Add(choice.Key, 0);
            if(choice.Value != this.netIdentity && !roundPoints.ContainsKey(choice.Value)) roundPoints.Add(choice.Value, 0);

            //player choose right answer -> +3 points
            if (choice.Value == this.netIdentity) roundPoints[choice.Key] += 3;
            //player choose own anser -> -1 point
            else if (choice.Key == choice.Value 
                || (sameAnswers.ContainsKey(choice.Value) && sameAnswers[choice.Value].Contains(choice.Key))) roundPoints[choice.Key] -= 1;
            //player choose answer of other player -> other player get +1 point
            else
            {
                roundPoints[choice.Value] += 1;       
                if(!sameAnswers.ContainsKey(choice.Value)) return;

                foreach (var player in sameAnswers[choice.Value])
                {
                    roundPoints[choice.Value] += 1;
                }
            }
        }

        foreach (var p in points)
        {
            Debug.Log(p.Key.netId + " Points: " + p.Value);
        }
        Player.LocalPlayer.RpcHighlightCard(this.netIdentity);

        pointsList = points.ToList();
        pointsList.Sort((pair1,pair2) => pair1.Value.CompareTo(pair2.Value));
        roundPointsList = roundPoints.ToList();
        roundPointsList.Sort((pair1,pair2) => pair1.Value.CompareTo(pair2.Value));

        UpdateScoreResultsOverlay();
        UpdatePlayerCanvas();

        StartCoroutine(waitAndShowResults());
    }

    /// <summary>
    /// Updates PlayerCanvas with new scores and ranking in all clients
    /// </summary>
    private void UpdatePlayerCanvas(){
        foreach(Player p in GetPlayers()){
            int idx = PlayerCount-1;
            foreach (KeyValuePair<NetworkIdentity, int> points in pointsList){
                String player = points.Key.GetComponent<Player>().playerName;
                String playerPoints = points.Value.ToString();
                p.TargetUpdatePlayerCanvasEntry(idx, player, playerPoints);
                idx--;
            }
        }

    }

    private IEnumerator waitAndShowResults()
    {
        yield return new WaitForSeconds(3);
        foreach(Player p in GetPlayers()){
            p.TargetResultOverlaySetActive(true);
        }
    }

    public void LogPlayerIsReady()
    {
        playersReady++;
        if (NetworkManager.singleton.numPlayers == playersReady){
            playersReady=0;
            CleanUpEvalPhase();
            ChangePhase();
        }
    }

    private void CleanUpEvalPhase(){
        Player.LocalPlayer.RpcDeleteQuestionCard();
        Player.LocalPlayer.RpcDeleteAllAnswerCards();

        answers.Clear();
        choices.Clear();

    }

    /// <summary>
    /// Updates ScoreResultsOverlay with new scores and ranking in all clients
    /// </summary>
    private void UpdateScoreResultsOverlay(){
        foreach(Player p in GetPlayers()){
            p.TargetUpdateScoreHeader(currentRound+1);
            int idx = PlayerCount-1;
            foreach (KeyValuePair<NetworkIdentity, int> roundPoints in roundPointsList){
                String player = roundPoints.Key.GetComponent<Player>().playerName;
                int playerPoints = roundPoints.Value;
                p.TargetUpdateTextPanelEntry(idx, player, playerPoints);
                idx--;
            }
        }
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
                StartRound();
                break;
            default:
                break;
        }
    }

    private void SendAnswers()
    {

        var startX = (answers.ToArray().Length * 125 + (answers.ToArray().Length - 1) * 20) / 2;

        var xPosition = startX - 62.5;
        int index = 0;
        
        KeyValuePair<NetworkIdentity, string>[] answersArray = answers.ToArray();
        
        ShuffleArray(answersArray);
        
        foreach (var answer in answersArray)
        {
            var cardGo = Instantiate(m_cardPrefab, new Vector3(0, -100, -(2+1*index)), Quaternion.Euler(0, 0, 0));
            var card = cardGo.GetComponent<Card>();
            card.text = answer.Value;
            card.choosen = answer.Key;
            card.type = Card.CardType.Answer;
            card.startFacedown = true;

            NetworkServer.Spawn(cardGo);
            card.RpcSlideToPosition(new Vector3((float)xPosition, -100, -2));
            card.RpcFlip(false,false,(float)(index*0.2+1));
            
            xPosition -= 145;
            
            index++;
        }
    }
    
    private void ShuffleArray<T>(T[] array)
    {
        // Knuth shuffle algorithm :: courtesy of Wikipedia :)
        for (int t = 0; t < array.Length; t++ )
        {
            T tmp = array[t];
            int r = Random.Range(t, array.Length);
            array[t] = array[r];
            array[r] = tmp;
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
